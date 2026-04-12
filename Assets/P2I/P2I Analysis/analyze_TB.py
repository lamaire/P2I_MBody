import json
from collections import defaultdict
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns

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

# print(df)
# print(df.groupby("distance").size())

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