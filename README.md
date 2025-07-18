# Visma Resource Shortage Manager

Console application on **.NET 8** for managing resource shortages in an organization.  
Data is stored in a **JSON file**, ensuring persistence between runs.  
No database is used.

---

## 📋 Table of Contents

- [🚀 Requirements](#-requirements)
- [🛠 Setup and Run](#-setup-and-run)
- [🎛 Available Commands](#-available-commands)
- [✅ Implemented Features](#-implemented-features)
- [📦 Shortage Model](#-shortage-model)
- [✅ Rules & Behavior](#-rules--behavior)
- [🧪 Unit Tests](#-unit-tests)
- [✨ Notes](#-notes)

---

## 🚀 Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download)

---

## 🛠 Setup and Run

1. Clone the repository:
   ```bash
   git clone https://github.com/Enysmen/VismaTask1.git
   ```
2. Navigate to the project directory:
   ```bash
   cd VismaTask1
   ```
3. Build the solution:
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run 
   ```

---

## 🎛 Available Commands

| Command  | Description                  | Parameters                          |
|----------|------------------------------|-------------------------------------|
| register | Register a new shortage      | --title, --room, --category, --priority |
| delete   | Delete a shortage            | --title, --room                     |
| list     | List shortages with filters  | --title, --from, --to, --category, --room |

---

## ✅ Implemented Features

- **Shortage model** includes:
  - `Title`: string
  - `Name`: string (who submitted)
  - `Room`: enum (MeetingRoom, Kitchen, Bathroom)
  - `Category`: enum (Electronics, Food, Other)
  - `Priority`: int (1–10)
  - `CreatedOn`: datetime

- **Persistence**:
  - All shortages are stored in a local JSON file.
  - Data is retained between application runs.

- **Smart registration**:
  - If a shortage with the same `Title` and `Room` exists:
    - If new `Priority` > old → it overrides.
    - Otherwise → warning is shown.

- **Deletion**:
  - Only the author or admin can delete a shortage.

- **Filtering**:
  - By partial title match (case-insensitive)
  - By `CreatedOn` date range (e.g., `--from 2025-01-01 --to 2025-01-31`)
  - By `Category`
  - By `Room`

---

## 📦 Shortage Model

| Property   | Type     | Description                        |
|------------|----------|------------------------------------|
| Title      | string   | Name of the shortage               |
| Name       | string   | User who created the shortage      |
| Room       | enum     | MeetingRoom, Kitchen, Bathroom     |
| Category   | enum     | Electronics, Food, Other           |
| Priority   | int      | Range from 1 (low) to 10 (critical)|
| CreatedOn  | DateTime | Timestamp of when shortage was created |

---

## ✅ Rules & Behavior

- **Duplicate prevention**:
  - Same `Title` + `Room` not allowed
  - If new `Priority` > existing → replace it
  - Else → warn user, do not register

- **Deletion**:
  - Only the creator or admin can delete

- **Listing**:
  - Admin sees all shortages
  - Users see only their own
  - Results sorted by priority descending

---

## 🧪 Unit Tests

Unit tests are implemented in a separate repository:[TestVismaTask1]([https://github.com/Enysmen/TestVismaTask1.git]).

---

## ✨ Notes

- UI is intentionally minimalistic (console only).
- Code follows OOP principles and .NET coding conventions.
- No database is used — only file-based storage.
- Application designed for internship task completion and demonstration of clean architecture and testability.

---
