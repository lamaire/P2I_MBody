## ⚙️ Procédure d'installation

### Clonnage du dépôt
- Se placer dans le dossier souhaité
- Cloner le répertoire : 
```js
git clone https://github.com/lamaire/P2I_MBody.git 
```
---
### Unity
- Ouvrir le projet sous la version **6000.2.7f2** de Unity
- Ouvrir la scène P2I dans Unity (File > Open Scene) :
  > Assets > Scene > P2IScene.unity
#### Lancer les tâches
- Appuyer sur "Play" ▶️
#### Lancer les tests unitaires
- Dans le **Test Runner** Unity (Window > General > Test Runner), sélectionner **EditMode**
- Appuyer sur **Run All**
---
### Scripts
#### Tâches Expérimentale (C#)
  > Assets > P2I > P2I Scripts
#### Tests Unitaires
  > Assets > Tests > EditMode
#### Analyse des données (Python)
  > P2I Analysis
---
### Analyse Python
- Créer et activer l’environnement Conda dans VS Code :
```js
conda env create -f environment.yml
conda activate p2i
```
