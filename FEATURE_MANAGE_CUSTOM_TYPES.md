# ????? Zarz?dzanie W?asnymi Typami Projektów - Dokumentacja

## Przegl?d Funkcjonalno?ci

U?ytkownicy mog? teraz zarz?dza? (edytowa? i usuwa?) w?asnymi typami projektów przez dedykowan? stron? **"Manage Project Types"**.

## Dost?p do Funkcji

### 1. **Z G?ównej Strony (MainPage)**
- Przycisk **"?? MANAGE TYPES"** w prawym górnym rogu
- Znajduje si? obok listy projektów

### 2. **Z Formularza Projektu (AddOrEditProject)**
- Przycisk **"?? MANAGE"** obok etykiety "CATEGORY"
- Bezpo?redni dost?p podczas tworzenia/edycji projektu

## Strona Zarz?dzania Typami

### Struktura Interfejsu

```
???????????????????????????????????????
?  MANAGE PROJECT TYPES               ?
???????????????????????????????????????
?                                     ?
?  SYSTEM TYPES                       ?
?  ?? Electronics                     ?
?  ?? Programming                     ?
?  ?? Mechanics                       ?
?  ?? Home                            ?
?  ?? Other                           ?
?                                     ?
?  CUSTOM TYPES                       ?
?  ?? Gardening          [›]          ?
?  ?? 3D Printing        [›]          ?
?  ?? Robotics           [›]          ?
?                                     ?
?  [? ADD CUSTOM TYPE]                ?
?                                     ?
???????????????????????????????????????
```

### Sekcja: System Types
- **Wygl?d**: Szare obramowanie, brak interakcji
- **Zawarto??**: 5 predefiniowanych typów
- **Status**: Tylko do odczytu (nie mo?na edytowa?/usuwa?)

### Sekcja: Custom Types
- **Wygl?d**: Zielone obramowanie (NeonAccent)
- **Interakcja**: Dotkni?cie otwiera menu zarz?dzania
- **Pusta lista**: Wy?wietla komunikat "No custom types yet"

## Funkcje Zarz?dzania

### ?? Edycja Typu

**Krok po kroku:**
1. Dotknij typ niestandardowy na li?cie
2. Wybierz **"?? Edit"** z menu
3. Dialog 1: Wprowad? now? nazw?
4. Dialog 2: Wprowad? nowy opis (opcjonalnie)
5. Potwierdzenie: "Project type updated successfully!"

**Walidacja:**
- ? Nazwa nie mo?e by? pusta
- ? Nazwa musi by? unikalna
- ? Nie mo?na edytowa? predefiniowanych typów

**Kod:**
```csharp
private async Task EditCustomType(ProjectType projectType)
{
    string? name = await DisplayPromptAsync(
        "Edit Type",
        "Enter new name:",
        initialValue: projectType.Name,
        maxLength: 100);

    if (string.IsNullOrWhiteSpace(name))
        return;

    string? description = await DisplayPromptAsync(
        "Edit Description",
        "Enter new description:",
        initialValue: projectType.Description ?? string.Empty);

    try
    {
        _projectTypeService.UpdateProjectType(projectType.Id, name, description);
        LoadProjectTypes();
        await DisplayAlert("Success", "Project type updated successfully!", "OK");
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", ex.Message, "OK");
    }
}
```

### ??? Usuwanie Typu

**Krok po kroku:**
1. Dotknij typ niestandardowy na li?cie
2. Wybierz **"??? Delete"** z menu
3. System sprawdza u?ycie typu w projektach
4. Wy?wietla odpowiedni komunikat:
   - **Typ jest u?ywany**: "This type is used by X project(s). Deleting it will prevent you from creating new projects with this type, but existing projects will keep their type. Continue?"
   - **Typ nie jest u?ywany**: "Are you sure you want to delete 'TypeName'?"
5. Potwierdzenie: "Delete" lub "Cancel"
6. Sukces: "Project type deleted successfully!"

**Logika Usuwania:**
```csharp
private async Task DeleteCustomType(ProjectType projectType)
{
    // Sprawd? u?ycie
    var projects = _projectService.GetAllProjects();
    var usageCount = projects.Count(p => p.ProjectTypeId == projectType.Id);

    string message = usageCount > 0
        ? $"This type is used by {usageCount} project(s). Deleting it will prevent you from creating new projects with this type, but existing projects will keep their type. Continue?"
        : $"Are you sure you want to delete '{projectType.Name}'?";

    bool confirm = await DisplayAlert(
        "Delete Type",
        message,
        "Delete",
        "Cancel");

    if (!confirm)
        return;

    try
    {
        _projectTypeService.DeleteCustomProjectType(projectType.Id);
        LoadProjectTypes();
        await DisplayAlert("Success", "Project type deleted successfully!", "OK");
    }
    catch (Exception ex)
    {
        await DisplayAlert("Error", ex.Message, "OK");
    }
}
```

### ? Dodawanie Typu

**Przycisk**: "? ADD CUSTOM TYPE" na dole strony

**Flow identyczny jak w formularzu projektu:**
1. Dialog z nazw? typu
2. Dialog z opisem (opcjonalnie)
3. Utworzenie i od?wie?enie listy

## Zachowanie Bazy Danych

### Model Changes

#### Przed:
```csharp
public int ProjectTypeId { get; set; }
```

#### Po:
```csharp
public int? ProjectTypeId { get; set; } // Nullable
```

### Foreign Key Behavior

```csharp
modelBuilder.Entity<Project>()
    .HasOne(p => p.Type)
    .WithMany()
    .HasForeignKey(p => p.ProjectTypeId)
    .OnDelete(DeleteBehavior.SetNull); // ? Ustaw NULL gdy typ zostanie usuni?ty
```

**Co to oznacza:**
- Gdy typ zostanie usuni?ty, projekty z tym typem maj? `ProjectTypeId = NULL`
- `Type` navigation property b?dzie `null`
- Projekty nadal istniej? i s? funkcjonalne
- W UI wy?wietlamy "NO TYPE" lub domy?ln? warto??

## Integracja z UI

### 1. **AddOrEditProject**
```csharp
// Przycisk "?? MANAGE" otwiera stron? zarz?dzania
private async void OnManageTypesClicked(object sender, EventArgs e)
{
    await Navigation.PushAsync(new ManageProjectTypesPage(_projectTypeService, _projectService));
}

// OnAppearing() prze?adowuje typy po powrocie
protected override void OnAppearing()
{
    base.OnAppearing();
    LoadProjectTypes();
}
```

### 2. **MainPage**
```csharp
// Przycisk w nag?ówku
private async void OnManageTypesClicked(object sender, EventArgs e)
{
    var projectTypeService = MauiProgram.Services?.GetService<IProjectTypeService>();
    if (projectTypeService != null)
    {
        await Navigation.PushAsync(new ManageProjectTypesPage(projectTypeService, _projectService));
    }
}
```

### 3. **ProjectDetailsPage**
```csharp
// Obs?uga NULL type
TypeLabel.Text = _project.Type?.Name?.ToUpper() ?? "NO TYPE";
```

## Rejestracja w DI

```csharp
// MauiProgram.cs
builder.Services.AddTransient<ManageProjectTypesPage>();
builder.Services.AddScoped<IProjectTypeService, ProjectTypeService>();
```

## Migracje Bazy Danych

### 1. **AddProjectTypeTable**
- Utworzenie tabeli `ProjectTypes`
- Seed 5 predefiniowanych typów
- Relacja Foreign Key do `Projects`

### 2. **MakeProjectTypeIdNullable**
- Zmiana `ProjectTypeId` na nullable
- Zmiana `OnDelete` na `SetNull`

**Komenda:**
```bash
cd ProjectPlanner.Data
dotnet ef migrations add MakeProjectTypeIdNullable
dotnet ef database update
```

## Scenariusze U?ycia

### Scenariusz 1: Edycja Nazwy Typu
```
1. U?ytkownik stworzy? typ "Ogród"
2. Chce zmieni? na "Gardening" (po angielsku)
3. Przechodzi do "Manage Types"
4. Klika na "Ogród"
5. Wybiera "Edit"
6. Wpisuje "Gardening"
7. Opcjonalnie zmienia opis
8. Zapisuje
9. Wszystkie projekty z tym typem widz? now? nazw?
```

### Scenariusz 2: Usuni?cie Nieu?ywanego Typu
```
1. U?ytkownik utworzy? typ "Test"
2. Nigdy nie u?y? go w projektach
3. Przechodzi do "Manage Types"
4. Klika na "Test"
5. Wybiera "Delete"
6. Potwierdza usuni?cie
7. Typ znika z listy
```

### Scenariusz 3: Usuni?cie U?ywanego Typu
```
1. U?ytkownik ma 3 projekty typu "Robotics"
2. Chce usun?? ten typ
3. Przechodzi do "Manage Types"
4. Klika na "Robotics"
5. Wybiera "Delete"
6. Widzi ostrze?enie: "This type is used by 3 project(s)..."
7. Potwierdza mimo ostrze?enia
8. Typ zostaje usuni?ty
9. Projekty nadal istniej?, ale pokazuj? "NO TYPE"
10. U?ytkownik mo?e edytowa? projekty i wybra? nowy typ
```

## Bezpiecze?stwo i Walidacja

### ? Co Jest Chronione:

1. **Predefiniowane typy**
   - Nie mo?na edytowa?
   - Nie mo?na usun??
   - Próba wywo?uje wyj?tek: "Cannot modify/delete predefined project types."

2. **Unikalne nazwy**
   - Nie mo?na utworzy? duplikatu
   - Próba wywo?uje wyj?tek: "Project type with name 'X' already exists."

3. **Puste nazwy**
   - Nazwa nie mo?e by? pusta
   - Próba wywo?uje wyj?tek: "Name cannot be empty"

### ? Co Nie Jest Blokowane (Celowo):

1. **Usuwanie u?ywanych typów**
   - Dozwolone z ostrze?eniem
   - Projekty zachowuj? dane, ale `Type = null`
   - Umo?liwia elastyczne zarz?dzanie

## Testowanie

### Test 1: Edycja Typu
```
? Zmie? nazw? typu
? Zmie? opis typu
? Sprawd? aktualizacj? w formularzu projektu
? Sprawd? aktualizacj? w istniej?cych projektach
```

### Test 2: Usuwanie Nieu?ywanego Typu
```
? Usu? typ bez projektów
? Sprawd? znikni?cie z listy
? Sprawd? brak w formularzu projektu
```

### Test 3: Usuwanie U?ywanego Typu
```
? Usu? typ z projektami
? Sprawd? ostrze?enie o liczbie projektów
? Potwierd? usuni?cie
? Sprawd? projekty (powinny mie? Type = null)
? Edytuj projekt i wybierz nowy typ
```

### Test 4: Walidacja
```
? Próba edycji predefiniowanego typu ? B??d
? Próba usuni?cia predefiniowanego typu ? B??d
? Próba utworzenia duplikatu nazwy ? B??d
? Próba zapisania pustej nazwy ? Anulowanie
```

### Test 5: Nawigacja
```
? Otwarcie z MainPage
? Otwarcie z AddOrEditProject
? Powrót do AddOrEditProject od?wie?a list?
? Dodanie typu w "Manage" pojawia si? w formularzu
```

## Wskazówki UX

### Komunikaty U?ytkownika:
- ? **Sukces**: "Project type updated/deleted successfully!"
- ? **B??d**: Wy?wietla tre?? wyj?tku z serwisu
- ?? **Ostrze?enie**: "This type is used by X project(s)..."

### Wizualne Wskazówki:
- **System Types**: Szare, nieaktywne
- **Custom Types**: Zielone, aktywne, ze strza?k? `›`
- **Empty State**: "No custom types yet"
- **Tip**: "Tip: You can also add types directly when creating a project"

## Podsumowanie Zmian w Plikach

### Nowe Pliki:
1. `ProjectPlanner\Pages\ManageProjectTypesPage.xaml`
2. `ProjectPlanner\Pages\ManageProjectTypesPage.xaml.cs`
3. `ProjectPlanner.Data\Migrations\[timestamp]_MakeProjectTypeIdNullable.cs`

### Zmodyfikowane Pliki:
1. `ProjectPlanner.Model\Project.cs` - nullable ProjectTypeId
2. `ProjectPlanner.Data\Contexts\ProjectContext.cs` - DeleteBehavior.SetNull
3. `ProjectPlanner.Service\ProjectTypeService.cs` - usuni?to blokad? usuwania
4. `ProjectPlanner\Pages\MainPage.xaml` - przycisk "Manage Types"
5. `ProjectPlanner\Pages\MainPage.xaml.cs` - handler dla przycisku
6. `ProjectPlanner\Pages\AddOrEditProject.xaml` - przycisk "Manage"
7. `ProjectPlanner\Pages\AddOrEditProject.xaml.cs` - OnAppearing, handler
8. `ProjectPlanner\helpers\TextTruncateConverter.cs` - IsNotNullConverter
9. `ProjectPlanner\MauiProgram.cs` - rejestracja ManageProjectTypesPage

## Architektura

```
UI Layer (MAUI)
?? MainPage ? [?? MANAGE TYPES]
?? AddOrEditProject ? [?? MANAGE] + [? Add own type]
?? ManageProjectTypesPage ? [Edit] [Delete] [Add]
         ?
Service Layer
?? IProjectTypeService
?? ProjectTypeService
    ?? GetAllProjectTypes()
    ?? GetCustomProjectTypes()
    ?? GetPredefinedProjectTypes()
    ?? AddCustomProjectType()
    ?? UpdateProjectType() ?? NEW
    ?? DeleteCustomProjectType() ??? NEW (modified)
         ?
Data Layer
?? IProjectTypeRepository
?? ProjectTypeRepository
         ?
Database (SQLite)
?? ProjectTypes (Id, Name, Description, IsCustom)
?? Projects (ProjectTypeId? ? FK ? ProjectTypes, ON DELETE SET NULL)
```

## Wykorzystanie

Funkcjonalno?? jest w pe?ni zintegrowana i gotowa do u?ycia! ??

U?ytkownicy mog?:
- ?? **Edytowa?** w?asne typy projektów
- ??? **Usuwa?** w?asne typy projektów (z ostrze?eniem gdy s? u?ywane)
- ? **Dodawa?** nowe typy z trzech miejsc (inline, MainPage, dedykowana strona)
- ?? **Przegl?da?** wszystkie typy w przejrzystym interfejsie
- ?? **Chronione** s? predefiniowane typy systemowe
