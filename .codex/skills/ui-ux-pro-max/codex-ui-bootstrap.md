# Codex Task - Frontend UI Documentation Bootstrap

## Goal

The CMS V2 backend architecture has already been designed.

Do not redesign the backend.

Do not modify backend models.

The goal of this task is to establish frontend architecture documentation before implementation.

The output of this task should be documentation only.

Do not implement business features.

Do not implement complex pages.

Do not start coding UI components.

Build documentation first.

---

# Required Deliverables

Create the following documentation structure:

```text
AGENTS.md

docs/
└── ui/
    ├── ui-architecture.md
    ├── component-rules.md
    ├── section-page.md
    ├── focus-tree.md
    ├── i18n.md
    └── codex-workflow.md
```

---

# Project Context

This system is not primarily a question bank.

This system is a teaching-design platform.

Questions, knowledge points, notes and summaries are teaching materials.

The primary object being organized is teaching structure.

The frontend should support efficient lesson preparation workflows.

---

# Architecture Philosophy

## Teaching Design First

The purpose of the system is:

```text
Teaching Structure Design
```

not:

```text
Question CRUD
```

Users organize:

```text
Teaching Ideas
Teaching Structures
Atomic Sections
Sections
```

rather than simply storing questions.

---

## Structure Should Emerge

Do not assume users create structures first.

Expected workflow:

```text
Collect Content
↓
Review Content
↓
Discover Teaching Patterns
↓
Create Atomic Sections
↓
Build Section Structure
```

The system must support gradual organization.

The system must not force early structural decisions.

---

## Attention Management

Attention management is a core design goal.

Future tree structures should support:

```text
Focus Workspace
```

allowing users to temporarily focus on one branch while hiding unrelated content.

---

# Frontend Technology Stack

Use:

```text
Vue 3
Vite
Tailwind CSS
shadcn-vue
Pinia
Vue Router
Vue I18n
```

Do not introduce unnecessary frameworks.

Keep the architecture simple.

---

# Internationalization Requirements

The UI will initially be developed in English.

Chinese support will be added later.

Therefore:

All visible UI text must use i18n keys.

Forbidden:

```vue
<Button>Save</Button>
```

Required:

```vue
<Button>{{ t("common.save") }}</Button>
```

The first version only requires English resources.

Chinese resources will be added later.

Language switching UI is not required yet.

However the architecture must support future localization.

---

# Frontend Layers

The frontend should be divided into:

```text
Pages
Components
Stores
APIs
Composables
```

Document the responsibility boundaries of each layer.

---

# State Ownership Rules

State should be placed according to usage scope.

Component State:

```text
Used by one component only.
```

Page State:

```text
Used by multiple components inside one page.
```

Store State:

```text
Used across multiple pages.
```

Important:

If you believe a state should be placed into a Store:

You must first explain:

```text
Which pages use it?
Why must it be shared?
```

Do not assume.

Wait for user confirmation.

---

# Component Rules

Document three component types:

## Presentation Components

Pure UI.

Examples:

```text
Badge
Breadcrumb
ToolbarButton
```

## Business Components

Business-aware components.

Examples:

```text
QuestionCard
ContentBlockCard
AtomicSectionCard
```

Business components may use composables.

## Business Container Components

Reusable business modules.

Examples:

```text
QuestionPicker
QuestionSearchPanel
```

Business container components may load data.

---

# API Rules

Previous rule:

```text
No component may call APIs.
```

This is too strict.

Use the following rule:

```text
Presentation Components
→ No API calls

Business Components
→ Prefer composables

Business Container Components
→ API calls allowed

Pages
→ API calls allowed

Composables
→ API calls allowed
```

Document this clearly.

---

# SectionPage Documentation

Create a dedicated document describing SectionPage.

SectionPage is the primary teaching-structure editing page.

The purpose is:

```text
Teaching Structure Editing
```

not:

```text
Material Collection
```

---

# Content Model Understanding

Document the following concepts:

## ContentBlock

Editable content unit.

Examples:

```text
Question
Knowledge Point
Summary
Note
```

ContentBlocks may open Word for editing.

## AtomicSection

Smallest teaching unit.

AtomicSections organize ContentBlocks.

AtomicSections do not directly contain editable document content.

## Section

Teaching structure composed of AtomicSections and ContentBlocks.

Sections organize content.

Sections do not directly edit document content.

---

# SectionPage Layout

Document the following layout:

```text
Toolbar

Left:
SectionStructurePanel

Center:
SectionWorkspace

Right:
SectionInspector
```

Left panel:

```text
Fixed width
Collapsible
Dockable
```

Center panel:

```text
Flexible width
Main workspace
Largest area
```

Right panel:

```text
Fixed width
Collapsible
Dockable
```

The center workspace should always receive the majority of user attention.

---

# Secondary Workflows

The following areas should not become primary regions inside SectionPage:

```text
Question Staging Area
Pending Atomic Sections
Temporary Collections
```

These are secondary workflows.

Access should be provided through:

```text
Toolbar Entry
Drawer
Dialog
Secondary Panel
```

not through permanent main regions.

---

# FocusTree Documentation

Create a dedicated FocusTree document.

Document:

```text
FocusTree
TagTree
SectionTree
ContentBlockTree
```

Business trees use FocusTree.

Business trees do not inherit FocusTree.

FocusTree provides:

```text
Focus Workspace
Breadcrumb
Back Navigation
Root Navigation
Attention Management
```

Business trees provide:

```text
Business Meaning
Node Rendering
Business Actions
```

Document composable usage:

```text
useFocusTree
```

---

# ComponentLabPage

Document a dedicated component testing page.

Purpose:

```text
Component Development
Mock Data Testing
UI Verification
```

Every reusable component should provide demo scenarios.

Examples:

```text
QuestionCard
FocusTree
SectionTree
AtomicSectionCard
```

---

# Mock Data First Workflow

Document the following workflow:

```text
Define DTO
↓
Create Mock Data
↓
Build Component
↓
Connect API
```

Components should not be built without representative mock data.

---

# Codex Workflow

Create a dedicated workflow document.

Before implementing any page or complex component:

Codex must first produce:

```text
Goal
Component Tree
Responsibilities
State Table
Event Table
Props Table
API List
File Plan
```

Wait for user confirmation before implementation.

---

# AGENTS.md Requirements

AGENTS.md should remain concise.

Do not duplicate all documentation inside AGENTS.md.

AGENTS.md should:

```text
Describe project goals
Describe key rules
Link to docs/ui/*
```

Keep AGENTS.md lightweight.

---

# Current Priority

Phase 1:

```text
Documentation
Project Structure
I18n Foundation
ComponentLabPage
FocusTree Prototype
SectionPage Layout
```

Phase 2:

```text
Section Editing Flow
ContentBlock Editing
AtomicSection Organization
```

Phase 3:

```text
Backend Integration
Saving
Preview
```

Phase 4:

```text
Drag & Drop
Advanced Animations
Optimization
```

---

# Final Principle

The frontend should help teaching structures emerge naturally.

The system should support structure formation.

The system should never force premature structure decisions.
