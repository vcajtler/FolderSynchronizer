# Folder Synchronization Program

## Overview

This C# program synchronizes the content of a source folder to a replica folder. The synchronization is one-way, ensuring that the replica folder exactly matches the content of the source folder. The program runs periodically at a specified interval and logs file creation, copying, and removal operations to a file and the console output.

## Usage

```bash
FolderSynchronizer.exe <sourceFolderPath> <replicaFolderPath> <logFilePath> <syncIntervalInSeconds>
