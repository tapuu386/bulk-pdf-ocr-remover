Recursive PDF OCR Layer Remover
A high-performance C# .NET automation utility built for bulk document security. This tool scans a source directory, identifies all PDF files, and strips their OCR (text) layers to make them non-searchable and non-copyable.

The standout feature is its ability to mirror the exact folder structure in the output directory, making it ideal for enterprise-level document archiving.

Key Features
OCR Layer Stripping: Converts searchable text layers into flattened, high-fidelity images.
Recursive Folder Processing: Automatically crawls through sub-folders to find and process PDFs.
Structure Mirroring: Recreates the original source directory tree in the output folder.
Privacy Focused: Prevents automated data scraping and text-indexing.
Bulk Processing: Built to handle hundreds of files efficiently using .NET streams.
Technical Stack
Language: C#
Framework: .NET 6.0 / .NET Core
Core Logic: Image Rasterization & PDF Reconstruction
How to Use
Configure Paths: Set your SourcePath (where searchable PDFs are) and OutputPath in the config.
Run Execution: The tool will begin mapping the folders.
Verification: Check the output folder; you will find the same sub-folder names with "Clean" image-only PDFs inside.
Example Workflow
Source/
├── Legal/
│   └── contract.pdf (Searchable)
└── Finance/
    └── invoice.pdf (Searchable)

Output/
├── Legal/
│   └── contract.pdf (Non-Searchable Image)
└── Finance/
    └── invoice.pdf (Non-Searchable Image)
