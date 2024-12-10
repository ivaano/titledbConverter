
## Export categories to tsv
```bash
cat MX.es.json| jq 'map(select(.category != null)) | .[] .category | flatten[]' | sort | uniq |  tr -d '"' | awk '{print $0"\t"$0}' > /home/ivan/RiderProjects/titledbConverter/titledbConverter/Datasets/categories.MX.es.tsv
````

## Filter by key
```bash
jq '.[] | select(.publisher == "Ubisoft")' US.en.json 
```

## Filter by category
```bash
cat MX.es.json | jq '.[] | select(.category | index("Herramientas"))'
```

## Generate list of region-languages
```bash
cat languages.json | jq 'to_entries[] | .key as $k | .value[] | $k + "." + . + ".json"'
```