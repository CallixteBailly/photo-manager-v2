# PRD — PhotoManager v3 : Cross-Platform Migration

> **Version:** 1.0
> **Date:** 14 avril 2026
> **Auteur:** Kallistos (CallixteBailly)
> **Statut:** Draft
> **Repo source:** [CallixteBailly/photo-manager-v2](https://github.com/CallixteBailly/photo-manager-v2) (fork de [PABERTHIER/photo-manager](https://github.com/PABERTHIER/photo-manager))

---

## 1. Vision

PhotoManager est une application desktop de gestion de photos locales (galerie, déduplication, déplacement, rotation, corrupted detection). Actuellement Windows-only via WPF.

L'objectif est de la rendre cross-platform (Windows, Linux, macOS) en conservant 100% des fonctionnalités existantes, la qualité du code (1708 tests, zero warnings), et les performances.

---

## 2. Contexte & Contraintes

### 2.1 État actuel

| Métrique | Valeur |
|----------|--------|
| LOC total | 137,500 |
| LOC commentaires | 2,900 (ratio 1.6%) |
| Fichiers C# | 322 |
| Tests unitaires | 189 classes, 1708 méthodes |
| Benchmarks | 7 classes BenchmarkDotNet |
| Framework | .NET 10.0, C# 13 |
| UI | WPF (9 XAML = 587 lignes) |
| Architecture | Clean Architecture (Domain → Application → Infrastructure → UI) |
| CI/CD | GitHub Actions (Build + Test + Release + CodeQL + Coverage) |
| Notifications | Microsoft.Toolkit.Uwp.Notifications |

### 2.2 Architecture actuelle

```
PhotoManager.Common          ← Helpers (Bitmap, Exif, Hashing, Video, Image)
  ↑
PhotoManager.Domain          ← Entities, Interfaces, Services métier
  ↑                           (Asset, Folder, AssetCreationService)
  │
PhotoManager.Application     ← Orchestration, Facade (IApplication)
  ↑
PhotoManager.Infrastructure  ← Repositories, CSV Database, Image Processing
  ↑
PhotoManager.UI              ← WPF (ViewModels, Controls, Windows, Converters)
```

### 2.3 Dépendances critiques Windows-only

| Dépendance | Couches touchées | Occurrences | Gravité |
|------------|-----------------|-------------|---------|
| `System.Windows.Media.Imaging.BitmapImage` | Domain, Common, Infra, App | 78+ | CRITIQUE |
| `System.Windows.Media.Rotation` | Domain, Common, Infra, App | 20+ | CRITIQUE |
| `System.Drawing.Bitmap` (GDI+) | Common (HashingHelper) | 3 | HAUTE |
| `BitmapFrame` / `BitmapMetadata` (WIC) | Common (ExifHelper) | 3 | HAUTE |
| `BitmapEncoder` (JpegBitmapEncoder, etc.) | Common (BitmapHelper) | 4 | HAUTE |
| `Process.Start("explorer.exe")` | UI (FindDuplicatedAssets) | 1 | MOYENNE |
| `Microsoft.Toolkit.Uwp.Notifications` | UI | 1 | MOYENNE |
| `MessageBox.Show` | UI | 3 | BASSE |
| `Dispatcher.Invoke/InvokeAsync` | UI | 4 | BASSE |
| FFMPEG .exe binaires | Common (VideoHelper) | 3 fichiers | MOYENNE |

### 2.4 Points forts à préserver

- Clean Architecture stricte (pas de dépendances inversées sauf BitmapImage)
- 1708 tests avec NSubstitute, NUnit, FixedDate
- BenchmarkDotNet pour la performance
- Serilog, DI, Reactive Extensions
- Zero warnings policy (TreatWarningsAsErrors)
- editorconfig strict (120 chars, 4 spaces, CRLF)
- AGENTS.md + CLAUDE.md pour les AI agents

### 2.5 Contraintes

- Le fork doit rester merge-compatible avec PABERTHIER/photo-manager autant que possible
- Les 1708 tests doivent continuer de passer à chaque étape de la migration
- Pas de régression de performance sur le catalog (SHA512, PHash)
- La base de données CSV doit rester compatible (pas de migration de données utilisateur)
- Les binaires ffmpeg doivent fonctionner sur les 3 OS

---

## 3. Décisions techniques

### 3.1 Framework UI : Avalonia UI

|Raison | Détail |
|--------|--------|
| API proche de WPF | XAML → AXAML, Window, UserControl, INotifyPropertyChanged, converters, command binding — migration mécanique |
| Cross-platform natif | Windows, Linux (GTK), macOS |
| MVVM compatible | Le pattern actuel (ViewModels + BaseViewModel) fonctionne tel quel |
| .NET 8+ | Compatible avec .NET 10.0 |
| Communauté active | 22K+ stars, packages NuGet stables |
| Pas de MAUI | MAUI impose un overhead mobile inutile, moins mature sur desktop |

### 3.2 Pipeline image : 100% Magick.NET

Actuellement deux pipelines:
- WPF/WIC (BitmapFrame, BitmapImage, BitmapEncoder) → JPG, PNG, GIF
- Magick.NET → HEIC

Décision: tout migrer sur Magick.NET qui supporte tous les formats sur toutes les plateformes.

### 3.3 Abstraction image dans le Domain

Remplacer `BitmapImage` par des types primitifs dans le Domain:

| Avant (WPF) | Après (Platform-agnostic) |
|-------------|--------------------------|
| `BitmapImage? ImageData` | `byte[]? ImageData` |
| `BitmapImage LoadThumbnail()` | `byte[] LoadThumbnail()` |
| `BitmapImage LoadOriginalImage()` | `ImageInfo LoadOriginalImage()` (nouveau record) |
| `System.Windows.Media.Rotation` | `Domain.Enums.ImageRotation` (nouveau enum) |
| `BitmapEncoder` pour thumbnails | `MagickImage.ToByteArray(format)` |

### 3.4 Nouveau record ImageInfo dans le Domain

```csharp
namespace PhotoManager.Domain;

public record ImageInfo(byte[] Data, int Width, int Height, ImageRotation Rotation);
```

Remplace les tuples (originalImage.PixelWidth, PixelHeight) et le passage de BitmapImage entre couches.

### 3.5 Services abstraits pour l'OS

```csharp
// Nouveau dans Domain/Interfaces/
public interface IFileExplorerService
{
    void OpenFileInExplorer(string filePath);
    void SelectFileInExplorer(string filePath);
}

public interface INotificationService
{
    void ShowNotification(string title, string message);
    void ShowProgress(string title, string message, int progress);
}

public interface IDialogService
{
    Task<bool> ShowConfirmAsync(string title, string message);
    Task ShowInfoAsync(string title, string message);
}
```

### 3.6 FFMPEG cross-platform

- Supprimer les binaires .rar/.exe du repo
- Utiliser les binaires système ou télécharger au runtime via [ffmpeg.ffmpeghq](https://ffmpeg.org/download.html)
- FFMpegCore supporte déjà la config dynamique du binary path
- Conditional compilation ou RuntimeInformation pour détecter l'OS

---

## 4. Périmètre fonctionnel

### 4.1 Feature parity (v3.0 = v2.0 + cross-platform)

Toutes les features existantes sont conservées:

- Catalogage d'images (JPG, PNG, GIF, BMP, DNG, ICO, JFIF, TIFF, WebP, HEIC)
- Détection de corruption et rotation
- Génération de thumbnails
- Hashing (SHA512, MD5, DHash, PHash) avec détection de doublons
- Recherche et suppression de doublons (3 modes)
- Copie/déplacement d'images entre dossiers
- Sync de dossiers
- Extraction de première frame vidéo (FFMPEG)
- Navigation par dossiers
- Tri multi-critères
- Viewer fullscreen
- Backup/restore de la database

### 4.2 Améliorations bonus (v3.1+)

Ces items sont des quick wins identifiés pendant l'audit:

- [ ] `async void` → `async Task` (2 occurrences: ThumbnailsUserControl, SyncAssetsWindow)
- [ ] AssetRepository God Class → split en ThumbnailRepository, FolderRepository
- [ ] Lock(_syncLock) → ReaderWriterLockSlim
- [ ] File.ReadAllText → streaming pour gros catalogs
- [ ] appsettings.json: paths hardcodés → variables d'environnement
- [ ] Tests: supprimer la référence à PhotoManager.UI
- [ ] Binaires ffmpeg dans git → Git LFS ou téléchargement runtime
- [ ] Résoudre les 30 TODOs existants

---

## 5. Architecture cible

```
PhotoManager.Common              ← Helpers (Magick.NET pur, plus aucun System.Windows/System.Drawing)
  ↑
PhotoManager.Domain              ← Entities, Interfaces, Enums (ImageRotation, ImageInfo)
  ↑                               100% platform-agnostic
  │
PhotoManager.Application         ← Orchestration, Facade
  ↑
PhotoManager.Infrastructure      ← Repositories, CSV DB, Image Processing (Magick.NET)
  ↑
PhotoManager.UI                  ← AVALONIA (ViewModels, Controls, Windows, Converters)
                                    + OS-specific services (FileExplorer, Notification, Dialog)
```

### 5.1 Changements de fichiers

| Action | Fichier | Détail |
|--------|---------|--------|
| NOUVEAU | Domain/Enums/ImageRotation.cs | Enum: Rotate0, Rotate90, Rotate180, Rotate270 |
| NOUVEAU | Domain/ImageInfo.cs | Record: (Data, Width, Height, Rotation) |
| NOUVEAU | Domain/Interfaces/IFileExplorerService.cs | Interface OS-agnostic |
| NOUVEAU | Domain/Interfaces/INotificationService.cs | Interface OS-agnostic |
| NOUVEAU | Domain/Interfaces/IDialogService.cs | Interface OS-agnostic |
| REWRITE | Domain/GlobalUsings.cs | Supprimer `System.Windows.Media.Imaging` |
| REWRITE | Common/GlobalUsings.cs | Supprimer `System.Windows.Media.Imaging`, `System.Drawing` |
| REWRITE | Common/BitmapHelper.cs | 100% Magick.NET, retourner byte[]/ImageInfo |
| REWRITE | Common/ExifHelper.cs | 100% Magick.NET (supprimer BitmapFrame/BitmapMetadata) |
| REWRITE | Common/HashingHelper.cs | Remplacer System.Drawing.Bitmap par MagickImage |
| REWRITE | Common/VideoHelper.cs | Binaires cross-platform |
| REWRITE | Domain/Asset.cs | `byte[]? ImageData` au lieu de `BitmapImage?` |
| REWRITE | Domain/Interfaces/IImageProcessingService.cs | Retourner ImageInfo/byte[] |
| REWRITE | Domain/Interfaces/IAssetRepository.cs | Retourner byte[] pour LoadThumbnail |
| REWRITE | Domain/AssetCreationService.cs | Utiliser ImageInfo au lieu de BitmapImage |
| REWRITE | Application/IApplication.cs | Supprimer System.Windows, retourner byte[] |
| REWRITE | Application/Application.cs | Supprimer System.Windows, retourner byte[] |
| REWRITE | Infrastructure/GlobalUsings.cs | Supprimer System.Windows |
| REWRITE | Infrastructure/ImageProcessingService.cs | Délèguer à BitmapHelper réécrit |
| REWRITE | Infrastructure/AssetRepository.cs | byte[] au lieu de BitmapImage |
| REWRITE | Infrastructure/ServiceCollectionExtensions.cs | Enregistrer les nouveaux services OS |
| NOUVEAU | Infrastructure/Services/WindowsFileExplorerService.cs | explorer.exe |
| NOUVEAU | Infrastructure/Services/LinuxFileExplorerService.cs | xdg-open / dbus-send |
| NOUVEAU | Infrastructure/Services/MacOSFileExplorerService.cs | open -R |
| NOUVEAU | Infrastructure/Services/WindowsNotificationService.cs | Windows notification |
| NOUVEAU | Infrastructure/Services/LinuxNotificationService.cs | libnotify |
| NOUVEAU | Infrastructure/Services/MacOSNotificationService.cs | macOS notification |
| NOUVEAU | Infrastructure/Services/AvaloniaDialogService.cs | Dialog via Avalonia |
| MIGRER | UI/ → UI.Avalonia/ | XAML → AXAML, Window → Window, UserControl → UserControl |
| MIGRER | Directory.Build.props | `<UseWPF>true</UseWPF>` → Avalonia targets |
| MIGRER | CI/CD (GitHub Actions) | Ajouter matrix: windows, ubuntu, macos |

---

## 6. Plan de migration détaillé

### Phase 0 — Préparation (Sprint 0)

Objectif: préparer le terrain sans casser l'existant.

| # | Tâche | Effort | Risque |
|---|-------|--------|--------|
| 0.1 | Créer la branche `feature/cross-platform` depuis `main` | 0.5h | Faible |
| 0.2 | Configurer CI matrix (windows + ubuntu + macos) en mode "build only" | 1h | Faible |
| 0.3 | Vérifier que les tests passent sur le CI actuel | 0.5h | Faible |
| 0.4 | Créer un tag `v2.x-stable` avant la migration | 0.5h | Faible |
| 0.5 | Ajouter les packages Avalonia au projet (sans les activer) | 0.5h | Faible |

**Livrable:** Branche prête, CI vert, tag de sécurité.

---

### Phase 1 — Purger le Domain (Sprint 1)

Objectif: le Domain ne contient plus aucune dépendance Windows.

| # | Tâche | Fichiers touchés | Effort | Risque |
|---|-------|-----------------|--------|--------|
| 1.1 | Créer `Domain/Enums/ImageRotation.cs` | 1 nouveau | 0.5h | Faible |
| 1.2 | Créer `Domain/ImageInfo.cs` (record) | 1 nouveau | 0.5h | Faible |
| 1.3 | Remplacer `System.Windows.Media.Rotation` par `ImageRotation` partout | ~20 fichiers | 3h | Moyen |
| 1.4 | Remplacer `BitmapImage` par `byte[]`/`ImageInfo` dans Asset.cs | Asset.cs | 1h | Moyen |
| 1.5 | Réécrire `IImageProcessingService.cs`: retourner `ImageInfo`/`byte[]` | Interface | 2h | Moyen |
| 1.6 | Réécrire `IAssetRepository.cs`: `LoadThumbnail` retourne `byte[]` | Interface | 1h | Moyen |
| 1.7 | Réécrire `AssetCreationService.cs`: utiliser `ImageInfo` | AssetCreationService.cs | 4h | Moyen |
| 1.8 | Supprimer `System.Windows` de `Domain/GlobalUsings.cs` | GlobalUsings.cs | 0.5h | Faible |
| 1.9 | Mettre à jour les tests Domain qui mock BitmapImage | ~15 fichiers de test | 4h | Moyen |
| 1.10 | Lancer tous les tests, corriger les breaks | Tests | 4h | Moyen |

**Livrable:** Domain 100% platform-agnostic, tous les tests passent.

---

### Phase 2 — Purger Common et Infrastructure (Sprint 2)

Objectif: Common et Infrastructure ne contiennent plus aucune dépendance Windows.

| # | Tâche | Fichiers touchés | Effort | Risque |
|---|-------|-----------------|--------|--------|
| 2.1 | Réécrire `BitmapHelper.cs`: 100% Magick.NET | BitmapHelper.cs (315 lignes) | 6h | Élevé |
| 2.2 | Réécrire `ExifHelper.cs`: 100% Magick.NET (supprimer BitmapFrame) | ExifHelper.cs (161 lignes) | 3h | Moyen |
| 2.3 | Réécrire `HashingHelper.CalculateDHash`: MagickImage au lieu de System.Drawing.Bitmap | HashingHelper.cs | 2h | Moyen |
| 2.4 | Supprimer `System.Windows` et `System.Drawing` de `Common/GlobalUsings.cs` | GlobalUsings.cs | 0.5h | Faible |
| 2.5 | Mettre à jour `ImageProcessingService.cs` | ImageProcessingService.cs | 2h | Moyen |
| 2.6 | Mettre à jour `AssetRepository.cs` (LoadThumbnail retourne byte[]) | AssetRepository.cs (760 lignes) | 3h | Moyen |
| 2.7 | Supprimer `System.Windows` de `Infrastructure/GlobalUsings.cs` | GlobalUsings.cs | 0.5h | Faible |
| 2.8 | Réécrire `VideoHelper.cs`: binaires cross-platform | VideoHelper.cs (125 lignes) | 4h | Élevé |
| 2.9 | Créer `IFileExplorerService` + implémentations (Windows/Linux/macOS) | 4 nouveaux fichiers | 3h | Faible |
| 2.10 | Créer `INotificationService` + implémentations | 4 nouveaux fichiers | 3h | Faible |
| 2.11 | Créer `IDialogService` | 1 nouveau fichier | 1h | Faible |
| 2.12 | Enregistrer les nouveaux services dans DI | ServiceCollectionExtensions.cs | 1h | Faible |
| 2.13 | Mettre à jour les tests Common/Infrastructure | ~50 fichiers de test | 8h | Élevé |
| 2.14 | Lancer tous les tests, corriger les breaks | Tests | 6h | Élevé |

**Livrable:** Common + Infrastructure 100% platform-agnostic, tous les tests passent.

---

### Phase 3 — Mettre à jour Application (Sprint 3)

Objectif: la couche Application ne référence plus Windows.

| # | Tâche | Fichiers touchés | Effort | Risque |
|---|-------|-----------------|--------|--------|
| 3.1 | Réécrire `IApplication.cs`: byte[] au lieu de BitmapImage | IApplication.cs | 1h | Faible |
| 3.2 | Réécrire `Application.cs`: byte[] au lieu de BitmapImage | Application.cs | 2h | Faible |
| 3.3 | Mettre à jour les tests Application | ~20 fichiers de test | 4h | Moyen |
| 3.4 | Lancer tous les tests | Tests | 2h | Faible |

**Livrable:** Application 100% platform-agnostic, tous les tests passent.

---

### Phase 4 — Migration UI vers Avalonia (Sprint 4-5)

Objectif: remplacer WPF par Avalonia, l'app tourne sur les 3 OS.

| # | Tâche | Fichiers touchés | Effort | Risque |
|---|-------|-----------------|--------|--------|
| 4.1 | Créer le projet `PhotoManager.UI.Avalonia` | Nouveau projet | 2h | Faible |
| 4.2 | Copier les ViewModels (presque inchangés) | 10 fichiers (1116 LOC) | 2h | Faible |
| 4.3 | Créer `App.axaml` + `App.axaml.cs` | App | 2h | Faible |
| 4.4 | Migrer les Converters (IValueConverter → IValueConverter Avalonia) | 5 fichiers (147 LOC) | 3h | Faible |
| 4.5 | Migrer `MainWindow` (XAML → AXAML) | MainWindow (62 + 451 LOC) | 6h | Moyen |
| 4.6 | Migrer `ThumbnailsUserControl` | Control (82 + 97 LOC) | 3h | Faible |
| 4.7 | Migrer `ViewerUserControl` | Control (77 + 102 LOC) | 4h | Moyen |
| 4.8 | Migrer `FolderNavigationControl` | Control (35 + 201 LOC) | 4h | Moyen |
| 4.9 | Migrer `FindDuplicatedAssetsWindow` | Window (91 + 145 LOC) | 4h | Moyen |
| 4.10 | Migrer `FolderNavigationWindow` | Window (38 + 81 LOC) | 3h | Faible |
| 4.11 | Migrer `SyncAssetsWindow` | Window (131 + 185 LOC) | 5h | Moyen |
| 4.12 | Migrer `AboutWindow` | Window (31 + 33 LOC) | 1h | Faible |
| 4.13 | Créer `ImageSourceConverter`: byte[] → Avalonia.Bitmap | 1 nouveau fichier | 2h | Faible |
| 4.14 | Implémenter `AvaloniaDialogService` | 1 nouveau fichier | 3h | Faible |
| 4.15 | Remplacer `Dispatcher` Avalonia | 4 occurrences | 1h | Faible |
| 4.16 | Remplacer `Microsoft.Toolkit.Uwp.Notifications` | INotificationService | 2h | Faible |
| 4.17 | Mettre à jour `Directory.Build.props`: conditionnel WPF/Avalonia | Build props | 2h | Moyen |
| 4.18 | Mettre à jour CI matrix: build + test sur 3 OS | GitHub Actions | 4h | Élevé |
| 4.19 | Créer les packages de publish (dotnet publish) pour 3 OS | CI + scripts | 4h | Moyen |
| 4.20 | Tests manuels sur les 3 OS | — | 8h | — |

**Livrable:** App fonctionnelle sur Windows, Linux, macOS.

---

### Phase 5 — Tests et Qualité (Sprint 6)

Objectif: coverage 100%, CI vert sur 3 OS.

| # | Tâche | Effort | Risque |
|---|-------|--------|--------|
| 5.1 | Mettre à jour les tests UI (mocks Avalonia au lieu de WPF) | 8h | Élevé |
| 5.2 | Supprimer la référence à PhotoManager.UI (WPF) dans Tests.csproj | 1h | Faible |
| 5.3 | Benchmarks: mettre à jour pour Avalonia | 4h | Moyen |
| 5.4 | Code coverage report sur 3 OS | 2h | Faible |
| 5.5 | Performance testing: comparer catalog speed WPF vs Avalonia | 4h | Moyen |
| 5.6 | Documentation Readme.md mise à jour | 2h | Faible |

**Livrable:** CI vert, tests passent sur 3 OS, coverage maintenu.

---

## 7. Estimation

| Phase | Durée | Effort total |
|-------|-------|-------------|
| Phase 0 — Préparation | 0.5 sprint | ~3h |
| Phase 1 — Domain | 1 sprint | ~20h |
| Phase 2 — Common + Infra | 1.5 sprint | ~37h |
| Phase 3 — Application | 0.5 sprint | ~9h |
| Phase 4 — UI Avalonia | 2 sprint | ~58h |
| Phase 5 — Tests + Qualité | 1 sprint | ~21h |
| **TOTAL** | **6.5 sprints** | **~148h** |

En sprints de 2 semaines (20h/semaine): environ 3.5 mois.

---

## 8. Mapping Avalonia vs WPF

Ce mapping sert de référence pour la Phase 4.

### 8.1 Contrôles

| WPF | Avalonia | Notes |
|-----|----------|-------|
| `Window` | `Window` | API identique |
| `UserControl` | `UserControl` | API identique |
| `Image` | `Image` | Source binding différent: `Bitmap` au lieu de `BitmapImage` |
| `ListBox` | `ListBox` | API identique |
| `ItemsControl` | `ItemsControl` | API identique |
| `ScrollViewer` | `ScrollViewer` | API identique |
| `Grid` | `Grid` | API identique |
| `StackPanel` | `StackPanel` | API identique |
| `WrapPanel` | `WrapPanel` | Package séparé: `Avalonia.Controls` |
| `TextBlock` | `TextBlock` | API identique |
| `TextBox` | `TextBox` | API identique |
| `Button` | `Button` | API identique |
| `Label` | `TextBlock` | Pas de Label natif, utiliser TextBlock |
| `ProgressBar` | `ProgressBar` | API identique |
| `Separator` | `Border` | Pas de Separator natif |

### 8.2 Converters

| WPF | Avalonia | Changement |
|-----|----------|------------|
| `IValueConverter` (System.Windows.Data) | `IValueConverter` (Avalonia.Data) | Namespace change |
| `Convert(object, Type, object, CultureInfo)` | `Convert(object, Type, object, CultureInfo)` | Signature identique |
| `Visibility` enum | `IsVisible` bool | Plus de Visibility.Collapsed, utiliser un bool |

### 8.3 Data Binding

| WPF | Avalonia | Notes |
|-----|----------|-------|
| `{Binding Property}` | `{Binding Property}` | Identique |
| `{Binding Property, Mode=TwoWay}` | Identique | Identique |
| `{Binding Property, Converter={StaticResource}}` | Identique | Identique |
| `UpdateSourceTrigger=PropertyChanged` | Par défaut | Avalonia update plus souvent |
| `INotifyPropertyChanged` | Identique | Pas de changement |

### 8.4 MVVM

| WPF | Avalonia |
|-----|----------|
| `ObservableCollection<T>` | Identique |
| `ICommand` | Identique |
| `RelayCommand` | Identique |
| `BaseViewModel` | Identique (juste changer les usings) |
| `NotifyPropertyChanged` | Identique |
| `Dispatcher.Invoke` | `Dispatcher.UIThread.Invoke` |

### 8.5 XAML → AXAML

| Changement | Exemple |
|------------|---------|
| Extension fichier | `.xaml` → `.axaml` |
| `x:Class` | Identique |
| `x:Name` | `x:Name` (inchangé) |
| `x:Static` | Identique |
| `Style TargetType` | Identique |
| `DataTemplate` | Identique |
| `ControlTemplate` | Identique |
| `Trigger` | Identique |
| Les properties custom WPF | Peuvent nécessiter des StyledProperty au lieu de DependencyProperty |

---

## 9. Risques et Mitigations

| Risque | Probabilité | Impact | Mitigation |
|--------|-------------|--------|-----------|
| Magick.NET performance vs WPF/WIC | Moyen | Moyen | Benchmark avant/après dans Phase 2 |
| Avalonia rendering différences visuelles | Faible | Faible | Design minimaliste actuel = peu de styling |
| FFMPEG binaires cross-platform | Moyen | Moyen | Tests sur 3 OS dès Phase 2 |
| Tests mocksBitmapImage cassent | Élevé | Élevé | Phase par phase avec tests verts à chaque étape |
| Merge conflicts avec PABERTHIER | Moyen | Moyen | Commits atomiques, PR régulières |
| Avalonia pas de support pour certaines APIs WPF | Faible | Faible | Abstractions créées en Phase 1-3 |
| CI/CD complexité 3 OS | Faible | Moyen | Matrix GitHub Actions bien documenté |

---

## 10. Dépendances NuGet finales

### Projet Avalonia UI

```xml
<PackageReference Include="Avalonia" />
<PackageReference Include="Avalonia.Desktop" />
<PackageReference Include="Avalonia.Themes.Fluent" />
<PackageReference Include="Avalonia.Fonts.Inter" />
<PackageReference Include="Avalonia.Xaml.Behaviors" />
<PackageReference Include="Avalonia.Diagnostics" />
```

### Common (inchangé sauf suppression de WPF refs)

```xml
<PackageReference Include="FFMpegCore" />
<PackageReference Include="Magick.NET-Q16-AnyCPU" />
<PackageReference Include="Microsoft.Extensions.Logging" />
<PackageReference Include="System.Numerics.Tensors" />
```

### Supprimés

```xml
<!-- Plus besoin -->
<!-- <UseWPF>true</UseWPF> -->
<!-- <PackageReference Include="Microsoft.Toolkit.Uwp.Notifications" /> -->
```

---

## 11. Critères d'acceptance

### Pour chaque phase

- [ ] Tous les tests existants passent
- [ ] Zero warnings (TreatWarningsAsErrors)
- [ ] CodeQL passe
- [ ] Coverage ne baisse pas

### Phase 4 (UI Avalonia) — critères additionnels

- [ ] App se lance sur Windows 11
- [ ] App se lance sur Ubuntu 24.04
- [ ] App se lance sur macOS 15 (ou dernière version disponible)
- [ ] Catalogage d'un dossier de 100+ images fonctionne
- [ ] Thumbnails s'affichent correctement
- [ ] Détection de doublons fonctionne (SHA512 + PHash)
- [ ] Copie/déplacement d'images fonctionne
- [ ] Sync de dossiers fonctionne
- [ ] Extraction de première frame vidéo fonctionne

---

## 12. Glossaire

| Terme | Définition |
|-------|-----------|
| WIC | Windows Imaging Component — codec image Windows-only |
| GDI+ | Graphics Device Interface+ — API graphique Windows-only |
| AXAML | Avalonia XAML — variante du XAML pour Avalonia |
| PHash | Perceptual Hash — hash basé sur la similarité visuelle |
| DHash | Difference Hash — hash basé sur les différences de pixels adjacents |
| Magick.NET | Binding .NET pour ImageMagick, cross-platform |

---

## 13. Appendices

### A. Fichiers XAML à migrer (9 fichiers, 587 lignes)

```
UI/App.xaml                          →  40 lignes
UI/Controls/FolderNavigationControl  →  35 lignes
UI/Controls/ThumbnailsUserControl    →  82 lignes
UI/Controls/ViewerUserControl        →  77 lignes
UI/Windows/AboutWindow               →  31 lignes
UI/Windows/FindDuplicatedAssetsWindow→  91 lignes
UI/Windows/FolderNavigationWindow    →  38 lignes
UI/Windows/MainWindow                →  62 lignes
UI/Windows/SyncAssetsWindow          → 131 lignes
```

### B. ViewModels à migrer (10 fichiers, 1116 lignes)

```
UI/ViewModels/ApplicationViewModel.cs           → 484 lignes (le plus gros)
UI/ViewModels/FindDuplicatedAssetsViewModel.cs   → 314 lignes
UI/ViewModels/BaseProcessViewModel.cs            →  78 lignes
UI/ViewModels/SyncAssetsViewModel.cs             →  53 lignes
UI/ViewModels/FolderNavigationViewModel.cs       →  52 lignes
UI/ViewModels/ViewModelExtensions.cs             →  33 lignes
UI/ViewModels/DuplicatedSetViewModel.cs          →  36 lignes
UI/ViewModels/DuplicatedAssetViewModel.cs        →  22 lignes
UI/ViewModels/SortableObservableCollection.cs    →  28 lignes
UI/ViewModels/BaseViewModel.cs                   →  16 lignes
```

### C. Code-behind à migrer (8 fichiers, 1295 lignes)

```
UI/Windows/MainWindow.xaml.cs                  → 451 lignes
UI/Windows/FindDuplicatedAssetsWindow.xaml.cs   → 145 lignes
UI/Windows/SyncAssetsWindow.xaml.cs             → 185 lignes
UI/Controls/FolderNavigationControl.xaml.cs     → 201 lignes
UI/Controls/ViewerUserControl.xaml.cs           → 102 lignes
UI/Controls/ThumbnailsUserControl.xaml.cs       →  97 lignes
UI/Windows/FolderNavigationWindow.xaml.cs       →  81 lignes
UI/Windows/AboutWindow.xaml.cs                  →  33 lignes
```

### D. Tests par catégorie

```
Tests/Unit/Domain/              → Tests métier purs (zero changement UI)
Tests/Unit/Common/              → Tests helpers (changer mocks BitmapImage → byte[])
Tests/Unit/Infrastructure/      → Tests repos (changer mocks BitmapImage)
Tests/Unit/Application/         → Tests orchestration (changer mocks)
Tests/Unit/UI/ViewModels/       → Tests ViewModels (presque inchangés)
Tests/Integration/Domain/       → Tests intégration métier (changer les setups)
Tests/Integration/Common/       → Tests intégration helpers
Tests/Integration/Infrastructure/ → Tests intégration repos
Tests/Integration/UI/ViewModels/ → Tests intégration ViewModels
```
