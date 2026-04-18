import json
from collections import defaultdict
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from datetime import datetime

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

# Export Excel
mean_df = df.groupby("distance", as_index=False)["reactionTime"].mean()
mean_df = mean_df.rename(columns={"reactionTime": "meanReactionTime"})

timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
file_name = f"resultats_VTI_{timestamp}.xlsx"

with pd.ExcelWriter(file_name, engine="openpyxl") as writer:
    df.to_excel(writer, sheet_name="Donnees_brutes", index=False)
    mean_df.to_excel(writer, sheet_name="Moyennes", index=False)

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