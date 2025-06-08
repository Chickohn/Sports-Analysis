import csv
from datetime import datetime

input_file = "football_matches.csv"
output_file = "sorted_football_matches.csv"

with open(input_file, newline='', encoding='utf-8') as infile:
    reader = csv.DictReader(infile)
    rows = list(reader)

# Sort rows by date descending (assuming format yyyy-mm-dd)
rows.sort(key=lambda r: datetime.strptime(r['date'], "%Y-%m-%d"), reverse=True)

with open(output_file, 'w', newline='', encoding='utf-8') as outfile:
    writer = csv.DictWriter(outfile, fieldnames=reader.fieldnames)
    writer.writeheader()
    writer.writerows(rows)

print(f"Sorted {len(rows)} rows by date (descending) and saved to {output_file}")