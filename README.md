# ManagementAirPort — ASP.NET MVC (Mini‑projet .NET) [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

Application web ASP.NET MVC pour **la gestion des activités aéroportuaires** : avions, vols, passagers et billets, avec authentification par rôles (administrateur, passager). Projet académique (ITBS) réalisé par **Imen Hadj Taieb** (2024–2025). [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

---

## 📌 Sommaire
- [Contexte & objectifs](#-contexte--objectifs)
- [Fonctionnalités](#-fonctionnalités)
- [Architecture & conception](#-architecture--conception)
- [Prérequis](#-prérequis)
- [Installation & démarrage](#-installation--démarrage)
- [Structure du projet](#-structure-du-projet)
- [Captures d’écran (extraits)](#-captures-décran-extraits)
- [Roadmap](#-roadmap)
- [Ressources & références](#-ressources--références)
- [Licence](#-licence)

---

## 🎯 Contexte & objectifs
Le secteur aérien exige une gestion fluide et fiable des opérations (ressources, sécurité, délais, expérience utilisateur). Ce projet vise à **centraliser et automatiser** la gestion aéroportuaire via une application web ASP.NET MVC :  
- Concevoir une application **intuitive et centralisée**.  
- **Automatiser** la création des vols, la réservation/enregistrement des passagers et la génération de billets.  
- **Réduire les erreurs** et les redondances via une **base de données cohérente**.  
- Offrir **traçabilité** et accès rapide à l’information selon les rôles. [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

---

## ✅ Fonctionnalités
**Authentification & rôles**  
- Inscription des passagers et authentification par rôle (**admin, passager**).  
- Contrôle d’accès aux fonctionnalités selon le rôle. [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

**Gestion des avions (CRUD)**  
- Ajouter, modifier, supprimer, consulter.  
- Recherche avec filtres. [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

**Gestion des passagers (CRUD)**  
- Deux types : **Staff** (personnel) et **Traveller** (voyageur).  
- Vérification d’identité (nom, passeport, email, etc.). [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

**Gestion des vols (CRUD)**  
- Filtres (destination, date, durée, …). [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

**Billets (tickets)**  
- Génération après enregistrement à un vol (infos vol, passager, siège).  
- Consultation / téléchargement par le passager.  
- Annulation ou modification (selon statut du vol). [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

**Besoins non fonctionnels**  
- Interfaces **conviviales**, **performances** correctes, **séonception
- **Pattern MVC** (Modèle–Vue–Contrôleur) pour une séparation claire des responsabilités.  
  - **Modèles** : gestion des données et interaction BD  
  - **Contrôleurs** : orchestration, logique, sélection des vues  
  - **Vues** : interface utilisateur [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)
- **UML** utilisé pour modéliser :  
  - **Cas d’utilisation** (global, acteurs et interactions)  
  - **Diagramme de classes** (entités, relations) [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)
- **Cycle en cascade** (analyse → conception → développement → tests) adapté au cadre académique. [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

---

## 🔧 Prérequis
- **.NET / ASP.NET MVC** (projet ASP.NET MVC classique). [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)  
- **Visual Studio** (IDE conseillé). [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)  
- **SQL Server** (SGBDR cible). [1](https://itbsnabel-my.sharepoint.com/personal/imen_hadjtaieb_itbs_tn/Documents/Microsoft%20Copilot%20Chat%20Files/Rapport__net.pdf)

> Sur la page GitHub, les langages détectés : **HTML ~68.9%**, **C# ~29.6%**, **CSS ~1.4%**, **JavaScript ~0.1%**. [2](https://github.com/Imen-Ht07/ManagementAirPort)

---

## 🚀 Installation & démarrage

> **NB** : Le dépôt contient un dossier `GestionAirPort`. Les étapes ci‑dessous s’appliquent à un projet MVC ASP.NET ; adapte-les selon ton `*.sln` et ta configuration. [2](https://github.com/Imen-Ht07/ManagementAirPort)

1. **Cloner le dépôt**
   ```bash
   git clone https://github.com/Imen-Ht07/ManagementAirPort.git
   cd ManagementAirPort/GestionAirPort
