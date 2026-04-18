## ⚙️ Procédure d'installation
### VS Code
- Se placer dans le dossier souhaité
- Cloner le répertoire : 
```js
git clone https://github.com/lamaire/P2I_MBody.git 
```
- Ouvrir les fichiers souhaités :

Scripts des tâches C# : 
  > Assets > P2I > P2I Scripts

Scripts d'analyse Python : 
  > P2I Analysis

- Création et activation de l’environnement Conda (pour les scripts d'analyse) :
```js
conda env create -f environment.yml
conda activate p2i
```
---
### Unity
- Ouvrir le projet sous la version **6000.2.7f2** de Unity
- Ouvrir la scène P2I dans Unity (File > Open Scene) :
  > Assets > Scene > P2IScene.unity
- Appuyer sur "Play" ▶️
