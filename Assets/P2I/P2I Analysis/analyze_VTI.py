import json
from collections import defaultdict
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

files = [
    "VTI_P001.json",
    "VTI_P002.json",
    "VTI_P003.json"
]

rows = []

for file_name in files:
    with open(file_name, "r", encoding="utf-8") as f:
        data = json.load(f)


    for trial in data["trials"]:
        distance = trial["distance"]          
        reaction_time = trial["reactionTime"]

        rows.append({
            "distance": distance,
            "reactionTime": reaction_time
        })

df = pd.DataFrame(rows)
df = df.sort_values("distance")

# print(df)
# print(df.groupby("distance").size())

plt.figure(figsize=(10, 6))

sns.boxplot(
    data=df,
    x="distance",
    y="reactionTime"
)

plt.xlabel("Distance (cm)")
plt.ylabel("Temps de réaction (ms)")
plt.title("VTI - Boxplots des TR par distance")
plt.grid(axis="y", alpha=0.3)
plt.tight_layout()
plt.show()