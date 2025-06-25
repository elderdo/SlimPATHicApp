# SlimPATHic

SlimPATHic is a utility for Windows that shortens and deduplicates the entries in your system and user PATH environment variables. It converts each path to its short (8.3) format, removes duplicates, and safely backs up the original PATH before making changes. This helps prevent issues caused by overly long or redundant PATH variables.

## Features

- Converts all PATH entries to their short (8.3) form
- Removes duplicate entries
- Backs up the original PATH with a timestamp before making changes
- Supports both user and system PATH variables (system changes require admin rights)

## Usage

Run the application. It will process and update your PATH variables, displaying status messages and backup information in the console.

> **Note:** Modifying the system PATH requires administrator privileges.