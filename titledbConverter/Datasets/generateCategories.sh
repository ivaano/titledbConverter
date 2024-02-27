#!/bin/bash

TITLEDB_PATH=$1
OUT_PATH=$2
LANG_FILE="${TITLEDB_PATH}/languages.json"


if [ $# -eq 1 ]; then
 echo "missing arguments"
 echo "Usage: $0 <path to titledb> <output path>"
 exit 1
fi

# Geta all region-language pairs
results=$(cat $LANG_FILE | jq -r 'to_entries[] | .key as $k | .value[] | $k + "." + .')

# Iterate over the results and generate the TSV files
for result in $results; do
  TSVFILE="${OUT_PATH}/categories.$result.tsv"
  if [ -f "$TSVFILE" ]; then
    echo -e "\e[1;33mFile $TSVFILE exists skipping.\e[0m"
  else 
    JSONFILE="${TITLEDB_PATH}/$result.json"
    echo -e "\e[32mExtracting categories from file: $JSONFILE\e[0m"
    echo "original	translated" > "$TSVFILE"
    cat "$JSONFILE" | jq 'map(select(.category != null)) | .[] .category | flatten[]' | sort | uniq |  tr -d '"' | awk '{print $0"\t"$0}' >> "$TSVFILE"
    sed -i '/^$/d' "$TSVFILE"
  fi
done

