
##Export categories to tsv
```bash
cat MX.es.json| jq 'map(select(.category != null)) | .[] .category | flatten[]' | sort | uniq |  tr -d '"' | awk '{print $0"\t"$0}' > /home/ivan/RiderProjects/titledbConverter/titledbConverter/Datasets/categories.MX.es.tsv
````

##Filter by 
```bash
jq '.[] | select(.publisher == "Nintendo")' US.en.json 
```