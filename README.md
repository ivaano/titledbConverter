
# TitleDb Converter

This program will allow you to merge all the titledb regions in one json file, like nut creates titles.json, but
with added properties, like editions, versions and so on, also a sqlite database can be created and the file
can be loaded to do queries faster.


## Features

- Download all regions from titledb github repo
- Merge All regions using a preferred region/language
- Add missing fields like otherApplicationId for Patches or DLC
- Add additional Editions where a title has multiple ids
- Ability to create a sqlite database

## Database diagram


## Sample queries

### Get a title with the categories on the same column
```sql
SELECT
    t.*,
    GROUP_CONCAT(c.Name, ', ') AS Categories
FROM Titles t
JOIN TitleCategory tc ON t.Id = tc.TitleId
JOIN Categories c ON tc.CategoryId = c.Id
WHERE t.ApplicationId = '0100000000010000'
GROUP BY t.TitleName;
```

### Get the Rating Contents of a title
```sql
SELECT rc.* FROM Titles t
JOIN TitleRatingContents tc ON t.Id = tc.TitleId
JOIN RatingContents rc ON tc.RatingContentId = rc.Id
WHERE t.ApplicationId = '01006A800016E000';
```

### Get the DLC from a title
```sql
SELECT * from Titles t
WHERE t.OtherApplicationId = '01006A800016E000'
AND t.ContentType = 130;
```

### Get the languges of a title
```sql
SELECT l.* from Titles t
JOIN TitleLanguages tl on t.Id = tl.TitleId
JOIN Languages l on tl.LanguageId = l.Id           
WHERE t.ApplicationId = '01006A800016E000';
```

### Get the titles released from Today (no future release dates)
```sql
SELECT t.ApplicationId, t.TitleName, t.ReleaseDate FROM Titles t
WHERE t.ContentType = 128
AND t.Region = 'US'
AND t.ReleaseDate <= date()
ORDER BY t.ReleaseDate DESC 
```

## Demo

### Download all json files from github
```bash
❯ titledbConverter.exe download F:\titsconverter
Using default config base url https://raw.githubusercontent.com/blawar/titledb/master/
46 regions found.
Starting download of F:\titsconverter\versions.txt (2660353 bytes)    
Download of F:\titsconverter\versions.txt completed!
cnmts.json ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 00:00:00
versions.json ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 00:00:00
    ncas.json ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 00:00:00
 versions.txt ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 00:00:00
   BG.en.json ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━ 100% 00:00:00            
```

### Merge all regions using US.en.json as preferred
```
❯ titledbConverter.exe merge F:\titsconverter -r US -l en
Missing save filename using default filename F:\titsconverter\titles.json
Loaded F:\titsconverter\languages.json in: 18.2237 ms
Loaded F:\titsconverter\versions.json in: 21.7806 ms
Loaded F:\titsconverter\cnmts.json in: 447.4255 ms
Loaded F:\titsconverter\ncas.json in: 706.8927 ms
Processing F:\titsconverter\US.en.json
Loaded F:\titsconverter\US.en.json in: 435.2929 ms
Adding 22042 titles from US-en region
Updating 0 titles from US-en region
...
Processing F:\titsconverter\RU.ru.json
Loaded F:\titsconverter\RU.ru.json in: 344.1909 ms
Adding 1 titles from RU-ru region
Updating 16839 titles from RU-ru region
Titles Count: 47670
Base Titles: 18792
DLC Titles: 18239
Update Titles: 10639
Save to: F:\titsconverter\titles.json
Elapsed time: 35749.196 ms
```

### Sample record (not an actual title but same structure)
```json
[
  {
    "id": "0100000000010000",
    "otherApplicationId": null,
    "ids": null,
    "nsuId": 70010000001130,
    "name": "Super Awesome Game\u2122",
    "version": "262144",
    "isBase": true,
    "isDlc": false,
    "isUpdate": false,
    "isDemo": false,
    "patchCount": 4,
    "dlcCount": 0,
    "size": 6025117696,
    "intro": "Embark on an adventure",
    "bannerUrl": "https://img.cdn.someserver.net/i/c42553b4fd0312c31e70ec7468c6c9bccd739f340152925b9600631f2d29f8b5.jpg",
    "category": [
      "Platformer",
      "Action"
    ],
    "description": "A big description here",
    "developer": null,
    "frontBoxArt": null,
    "iconUrl": "https://img.cdn.someserver.net/i/ad4d31f664a1ce704f0219da2805f8459595bc3c01c3f04df2e32ba34a05b8c6.jpg",
    "key": null,
    "languages": [
      "de",
      "en",
      "es",
      "fr",
      "it",
      "ja",
      "nl",
      "ru",
      "zh"
    ],
    "numberOfPlayers": 2,
    "publisher": "Big Publisher",
    "rating": 10,
    "ratingContent": [
      "Cartoon Violence",
      "Comic Mischief"
    ],
    "region": "US",
    "language": "en",
    "releaseDate": 20171027,
    "rightsId": "01000000000100000000000000000003",
    "screenshots": [
      "https://img.cdn.someserver.net/i/c497547957d9dd3668e891aa97ff4899a3f40bd1bd430020f8cbdf673f02bdeb.jpg",
      ...
    ],
    "editions": null,
    "regions": [
      "AR",
      ...
      "ZA"
    ],
    "versions": [
      {
        "versionNumber": 65536,
        "versionDate": "2017-10-26"
      },
      {
        "versionNumber": 131072,
        "versionDate": "2017-11-30"
      },
      {
        "versionNumber": 196608,
        "versionDate": "2018-02-22"
      },
      {
        "versionNumber": 262144,
        "versionDate": "2019-04-25"
      }
    ],
    "cnmts": [
      {
        "contentEntries": [
          {
            "buildId": "3CA12DFAAF9C82DA064D1698DF79CDA100000000000000000000000000000000",
            "ncaId": "0f26bd42cae0e4cefda4b5bbf7ae3d50",
            "type": 1
          },
          ...
        ],
        "MetaEntries": null,
        "otherApplicationId": "0100000000010800",
        "requiredApplicationVersion": 0,
        "requiredSystemVersion": 201392128,
        "titleId": "0100000000010000",
        "titleType": 128,
        "version": 0
      }
    ],
    "ncas": [
      {
        "ncaId": "0f26bd42cae0e4cefda4b5bbf7ae3d50",
        "buildId": "3CA12DFAAF9C82DA064D1698DF79CDA100000000000000000000000000000000",
        "contentIndex": 0,
        "contentType": 0,
        "cryptoType": 2,
        "cryptoType2": 3,
        "isGameCard": 0,
        "keyIndex": 0,
        "rightsId": "01000000000100000000000000000003",
        "sdkVersion": 50659584,
        "size": 5539168256,
        "titleId": "0100000000010000"
      },
     ...
    ]
  }
]
```

and we can use `jq` to query the json file and get some info out of it

Examples:
### How many base titles are there
```
jq '.[] | select(.isBase == true) | .id' titles.json  | wc
  18792   18792  357048
```

### How many DLCS are there
```
jq '.[] | select(.isDlc == true) | .id' titles.json  | wc
  18239   18239  346541
```

### Do we have Titles with no name?
```
 jq '.[] | select(.isBase == true and .name == null) | .id' titles.json | wc
```

### Get a list of titles with no updates
```
jq '.[] | select(.isBase == true and (.version| type == "string" and tonumber == 0)) | .id' titles.json
"0100FF901F160000"
"0100FFB00CAF4000"
"0100FFB015C6E000"
"0100FFD00FDF8000"
...
```

### Get a list of titles that have multiple editions
```
jq '.[] | select(.editions | length > 0 ) | {id, name}' titles.json
```

```json
{
  "id": "0100FBD01884D002",
  "name": "衣装 「AKIBA'S TRIP 文月瑠衣」"
}
{
  "id": "0100FD70134FB003",
  "name": "早期解鎖訓練項目──與夢幻的基鈕特戰隊隊員對戰！？"
}
```



