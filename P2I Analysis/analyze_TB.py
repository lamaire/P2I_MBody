import json
from collections import defaultdict
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from datetime import datetime

files = [
    "TB_P001.json",
    "TB_P002.json",
    "TB_P003.json"
]

rows = []

for file_name in files:
    with open(file_name, "r", encoding="utf-8") as f:
        data = json.load(f)


    for trial in data["trials"]:
        true_delay = trial["trueDelay"]          
        estimated_delay = trial["estimatedDelay"]

        rows.append({
            "trueDelay": true_delay,
            "estimatedDelay": estimated_delay
        })

df = pd.DataFrame(rows)
df = df.sort_values("trueDelay")

# Export Excel
mean_df = df.groupby("trueDelay", as_index=False)["estimatedDelay"].mean()
mean_df = mean_df.rename(columns={"estimatedDelay": "meanEstimatedDelay"})

timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
file_name = f"resultats_TB_{timestamp}.xlsx"

with pd.ExcelWriter(file_name, engine="openpyxl") as writer:
    df.to_excel(writer, sheet_name="Donnees_brutes", index=False)
    mean_df.to_excel(writer, sheet_name="Moyennes", index=False)

plt.figure(figsize=(10, 6))

sns.boxplot(
    data=df,
    x="trueDelay",
    y="estimatedDelay"
)

plt.xlabel("Délai réel (ms)")
plt.ylabel("Délai estimé (ms)")
plt.title("TB - Boxplots des délais estimés par délai réel")
plt.grid(axis="y", alpha=0.3)
plt.tight_layout()
plt.show()