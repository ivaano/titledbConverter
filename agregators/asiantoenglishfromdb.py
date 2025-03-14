import json
import os
import re
import sys
import unicodedata
import xml.etree.ElementTree as ET
import sqlite3
from ollama import chat
from ollama import ChatResponse
from pathlib import Path


class Translator:
    def __init__(self, model='gemma2:27b'):
        self.model = model
        self.translation_cache = {}

    @staticmethod
    def is_non_english(text):
        if not isinstance(text, str):
            return False

        for char in text:
            category = unicodedata.category(char)
            if category.startswith('Lo'):  # Letter, other (ideographs)
                name = unicodedata.name(char, '')
                if ("CJK UNIFIED IDEOGRAPH" in name or
                        "HIRAGANA" in name or
                        "KATAKANA" in name or
                        "HANGUL" in name):
                    return True
            elif category.startswith('H'):  # H letter category, contains hiragana, katakana, and hangul
                return True
            elif category.startswith('I'):  # I category contains ideographic number and symbols
                return True
        return False

    @staticmethod
    def extract_translation(text):
        pattern = r"Translation:\s*(?:[\|](.*?)[\|]|(\S*))(?:\s*|$)"
        match = re.search(pattern, text, re.MULTILINE)
        if match:
            if match.group(1):
                return match.group(1)
            elif match.group(2):
                return match.group(2)
        return ""

    def translate_string(self, message):
        if self.is_non_english(message):
            response: ChatResponse = chat(model=self.model, messages=[
                {
                    'role': 'system',
                    'content': 'Focus solely on providing the direct translation of the input text.',
                },
                {
                    'role': 'system',
                    'content': 'Do not include additional explanations, commentary, or formatting.',
                },
                {
                    'role': 'system',
                    'content': 'Aim for accuracy and natural-sounding translations.',
                },
                {
                    'role': 'system',
                    'content': 'If the input text is unclear or contains errors, indicate that in the output.',
                },
                {
                    'role': 'system',
                    'content': 'All output should begin with Translation: followed by the translated text enclosed in pipes |.',
                },
                {
                    'role': 'user',
                    'content': 'Text to translate to english:' + message,
                }
            ])

            message = self.extract_translation(response.message.content)
        return message

    def translate_with_cache(self, message):
        if message in self.translation_cache:
            return self.translation_cache[message]
        else:
            translated = self.translate_string(message)
            self.translation_cache[message] = translated
            if translated is None:
                return message
            print(f"Translated '{message}' to '{translated}'")
            return translated


class TitleHelper:
    @staticmethod
    def extract_title_and_revision(full_title):
        if not full_title:
            return "", None

        revision_pattern_parenthesis = r"\(.*?\)"
        revision_pattern = r"-([A-Za-z0-9]+)$"

        no_rev_parenthesis_title = re.sub(revision_pattern_parenthesis, "", full_title).strip()

        match = re.search(revision_pattern, full_title)

        if not match:
            clean_title_name = re.sub(r"\[.*?]", "", no_rev_parenthesis_title).strip()
            return clean_title_name, None

        revision = match.group(1)
        clean_title = re.sub(revision_pattern, "", no_rev_parenthesis_title).strip()

        return clean_title, revision


class FileHandler:
    @staticmethod
    def check_file_exists(file_path_str):
        file_path = os.path.abspath(file_path_str)
        return os.path.isfile(file_path)

    @staticmethod
    def load_json_to_list(file_path):
        try:
            with open(file_path, 'r', encoding='utf-8') as file:
                data = json.load(file)

            # Convert the dictionary of records into a list of dictionaries
            records_list = []
            for key, value in data.items():
                record = {'id': key, **value}
                records_list.append(record)

            return records_list

        except FileNotFoundError:
            print(f"Error: The file at {file_path} was not found.")
            return []

        except json.JSONDecodeError:
            print(f"Error: The file at {file_path} is not a valid JSON file.")
            return []

    @staticmethod
    def create_initial_xml(file_path):
        root = ET.Element('releases')
        tree = ET.ElementTree(root)
        with open(file_path, 'wb') as f:
            tree.write(f, encoding='utf-8', xml_declaration=True, short_empty_elements=False)

    @staticmethod
    def extract_and_validate_locale(input_string):
        with open('languages.json') as f:
            supported_locales = json.load(f)
        parts = input_string.split('.')

        if len(parts) != 2:
            raise ValueError("Input string format is incorrect. Expected 'REGION.LANG.json'")

        region, language = parts

        if region in supported_locales and language in supported_locales[region]:
            return region, language
        else:
            raise ValueError(f"Invalid region '{region}' or language '{language}' for the given input string.")


class XMLHandler:
    def __init__(self, translator):
        self.translator = translator
        self.title_helper = TitleHelper()

    def parse_releases_xml(self, file_path):
        tree = ET.parse(file_path)
        root = tree.getroot()

        releases_data = {}

        for release in root.findall('release'):
            titleid = release.find('titleid').text
            clean_title, revision = self.title_helper.extract_title_and_revision(titleid)
            release_info = {
                'id': release.findtext('id', ''),
                'name': release.findtext('name', ''),
                'publisher': release.findtext('publisher', ''),
                'region': release.findtext('region', ''),
                'languages': release.findtext('languages', ''),
                'group': release.findtext('group', ''),
                'imagesize': int(release.findtext('imagesize', '0')),
                'serial': release.findtext('serial', ''),
                'titleid': clean_title,
                'version': int(release.findtext('revision', '0')),
                'imgcrc': release.findtext('imgcrc', ''),
                'idcrc': release.findtext('idcrc', ''),
                'filename': release.findtext('filename', ''),
                'releasename': release.findtext('releasename', ''),
                'trimmedsize': int(release.findtext('trimmedsize', '0')),
                'firmware': release.findtext('firmware', ''),
                'type': int(release.findtext('type', '0')),
                'card': release.findtext('card', ''),
                'notes': release.findtext('notes', '')
            }
            releases_data[clean_title] = release_info

        return releases_data

    def append_release_to_xml(self, root, title, region, language):
        release_element = ET.SubElement(root, 'release')
        ET.SubElement(release_element, "name").text = self.translator.translate_with_cache(title['name'])
        ET.SubElement(release_element, "titleid").text = title['id']
        ET.SubElement(release_element, "version").text = title['version']
        ET.SubElement(release_element, "publisher").text = self.translator.translate_with_cache(title['publisher'])
        ET.SubElement(release_element, "region").text = region
        ET.SubElement(release_element, "languages").text = language


class DatabaseHandler:
    def __init__(self, db_path):
        self.connection = sqlite3.connect(db_path)
        self.connection.row_factory = sqlite3.Row

    def get_titles_from_db(self):
        cursor = self.connection.cursor()
        cursor.execute("SELECT * FROM Titles")

        rows = cursor.fetchall()
        titles_dict = {row['ApplicationId']: dict(row) for row in rows}
        return titles_dict

    def close(self):
        self.connection.close()


class TitleProcessor:
    def __init__(self, translator, xml_handler, file_handler):
        self.translator = translator
        self.xml_handler = xml_handler
        self.file_handler = file_handler

    def load_already_translated(self, already_translated_dir, nswdb_data):
        if not already_translated_dir:
            return nswdb_data

        directory = Path(already_translated_dir)
        if not directory.exists():
            return nswdb_data

        xml_files = [file for file in directory.iterdir() if file.suffix == '.xml']
        result_dict = nswdb_data.copy()

        for xml_file in xml_files:
            already_converted = self.xml_handler.parse_releases_xml(xml_file)
            result_dict.update(already_converted)

        return result_dict

    def get_titles_to_translate(self, region_file, nswdb_file, already_translated_dir, output_file, db_handler):
        nswdb_data = self.xml_handler.parse_releases_xml(nswdb_file)

        if self.file_handler.check_file_exists(output_file):
            already_converted = self.xml_handler.parse_releases_xml(output_file)
            nswdb_dict = {**already_converted, **nswdb_data}
        else:
            nswdb_dict = nswdb_data.copy()

        nswdb_dict = self.load_already_translated(already_translated_dir, nswdb_dict)
        region_titles = self.file_handler.load_json_to_list(region_file)

        titles_to_process = []
        titledb_titles = db_handler.get_titles_from_db()

        for title in region_titles:
            title_id = title.get('id')
            if title_id is None or title_id in nswdb_dict:
                continue

            if title_id in titledb_titles:
                contains_non_english = self.translator.is_non_english(titledb_titles[title_id]['TitleName'])
                if not contains_non_english:
                    continue

            titles_to_process.append(title)

        return titles_to_process

    def process_titles(self, titles, file_path, region, language, overwrite=False):
        if overwrite or not self.file_handler.check_file_exists(file_path):
            self.file_handler.create_initial_xml(file_path)

        current_titles = self.xml_handler.parse_releases_xml(file_path)
        tree = ET.parse(file_path)
        root = tree.getroot()

        record_count = 0
        records_since_write = 0

        for title in titles:
            if title['id'] in current_titles:
                continue

            self.xml_handler.append_release_to_xml(root, title, region, language)
            record_count += 1
            records_since_write += 1
            print(f"Record count: {record_count}")

            if records_since_write >= 20:
                with open(file_path, 'wb') as f:
                    tree.write(f, encoding='utf-8', xml_declaration=True, method="xml", short_empty_elements=False)
                print(f"Added {records_since_write} records to {file_path}")
                records_since_write = 0

        if records_since_write > 0:
            with open(file_path, 'wb') as f:
                tree.write(f, encoding='utf-8', xml_declaration=True, method="xml", short_empty_elements=False)
        print(f"Added {record_count} records to {file_path}")


class AsianToEnglishTranslator:
    def __init__(self):
        #gemma2 is good and fast, a good alternative  aya-expanse:32b but is slower
        self.translator = Translator(model="gemma2:27b")
        self.file_handler = FileHandler()
        self.xml_handler = XMLHandler(self.translator)

    def run(self, args):
        if len(args) < 5:
            self.print_usage()
            return

        region_file = args[1]

        try:
            region_lang = Path(region_file).stem
            region, language = self.file_handler.extract_and_validate_locale(region_lang)
            print(f"Region: {region}, Language: {language}")
        except ValueError as e:
            print(e)
            return

        nswdb_file = args[2]
        titledb_path = args[3]
        already_translated_dir = args[4] if len(args) > 4 else None
        output_file = f"{region}.{language}.xml"

        if not (self.file_handler.check_file_exists(nswdb_file) and self.file_handler.check_file_exists(titledb_path)):
            print(f"File {nswdb_file} or {titledb_path} does not exist.")
            return

        db_handler = DatabaseHandler(titledb_path)
        try:
            title_processor = TitleProcessor(self.translator, self.xml_handler, self.file_handler)
            titles = title_processor.get_titles_to_translate(
                region_file,
                nswdb_file,
                already_translated_dir,
                output_file,
                db_handler
            )
            title_processor.process_titles(titles, output_file, region, language, False)
        finally:
            db_handler.close()

    def print_usage(self):
        print("This script is used to translate title name and publisher from a region file to English")
        print("and create an xml with the same attributes as th nswdb. ")
        print("A local instance of ollama needs to be running, I've used gemma2 model for the translation. ")
        print("but a good alternative is aya-expanse:32b. ")
        print("nswdb.xml and titledb.db are required to translate only titles not in those databases.")
        print("")
        print("Usage: python script.py <region_file> <nswdb_file> <titledb_path> [<already_translated_folder>]")
        print("")
        print("  <already_translated_folder> is optional and used to check if titles have already been translated from other languages.")
        print("")
        print("Example ./asiantoenglishfromdb.py JP.ja.json nswdb.xml titledb.db regions_translated/")


if __name__ == "__main__":
    translator_app = AsianToEnglishTranslator()
    translator_app.run(sys.argv)