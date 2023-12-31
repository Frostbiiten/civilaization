import os
import json
import requests_cache
from datetime import datetime
from dotenv import load_dotenv

load_dotenv(".env")
user = os.getenv("geouser")

session = requests_cache.CachedSession('grid_cache_6')

width = 55
height = 110

ids = {'NULL' : 0}
ids3 = []
grid = []

keys = ["arihan10", "frostbiiten", "edemh"]

counter = 0
for y in range(height):
    row = []
    for x in range(width - 1):
        user = keys[counter % 3]

        basey = y 
        if (x % 2 == 0): basey += 0.5
        params = {'username' : user, 'lat' : -(x / width * 180 - 90), 'lng' : basey / height * 360 - 180}
        req = session.get('http://api.geonames.org/countryCodeJSON', params=params)
        response = json.loads(req.content)
        if "status" in response:
            id = 0
        else:
            code = response["countryCode"]
            if code in ids:
                id = ids[code]
            else:
                id = len(ids)
                ids[code] = id
                ids3.append(code)
        row.append(id)
        p = ((y * width + x) / (width * height) * 100)
        if (p % 2 < 0.000001):
            print(req.text)
            print(p)
        counter  += 1
    grid.append(row)

dump_data = {"width": width, "height": height, "ids": ids3, "grid": grid}
f = open(f"tiledata {datetime.today().strftime('%Y-%m-%d %H-%M-%S')}.json", "w")
json.dump(dump_data, f, indent=4)
f.close()
