# GitHub Upload Guide

This guide will walk you through the process of uploading your Semantic Kernel Clothing Analysis project to GitHub.

## Prerequisites

1. A GitHub account
2. Git installed on your computer
3. Basic knowledge of Git commands

## Step 1: Initialize Git Repository (if not already done)

Navigate to your project directory and initialize a Git repository:

```bash
cd /path/to/SK/Shop
git init
```

## Step 2: Create a New GitHub Repository

1. Go to [GitHub](https://github.com) and sign in to your account
2. Click on the "+" icon in the top-right corner and select "New repository"
3. Enter a repository name (e.g., "semantic-kernel-clothing-analysis")
4. Optionally add a description
5. Choose if the repository should be public or private
6. Do NOT initialize the repository with a README, .gitignore, or license (we already have these files)
7. Click "Create repository"

## Step 3: Add Your Files to Git

Add all project files to the Git repository:

```bash
git add .
```

## Step 4: Commit Your Files

Create an initial commit with all your files:

```bash
git commit -m "Initial commit of Semantic Kernel Clothing Analysis project"
```

## Step 5: Link to GitHub Repository

Connect your local repository to the GitHub repository you just created:

```bash
git remote add origin https://github.com/your-username/semantic-kernel-clothing-analysis.git
```

Replace `your-username` with your GitHub username and `semantic-kernel-clothing-analysis` with your repository name if different.

## Step 6: Push to GitHub

Push your code to GitHub:

```bash
git push -u origin main
```

Note: If you're using an older version of Git, the default branch might be `master` instead of `main`. In that case, use:

```bash
git push -u origin master
```

## Step 7: Verify on GitHub

Go to your GitHub repository page to confirm that all files have been uploaded successfully.

## Additional Tips

### Setting Up GitHub Pages for Presentation

To make your presentation viewable online:

1. Go to your GitHub repository
2. Click on "Settings"
3. Scroll down to the "GitHub Pages" section
4. Under "Source", select the branch (main or master)
5. Click "Save"
6. After a few minutes, your presentation will be available at `https://your-username.github.io/semantic-kernel-clothing-analysis/presentation.html`

### Managing Sensitive Information

Remember that your `AZURE_OPENAI_API_KEY` is sensitive information and should not be committed to your repository. It's stored as an environment variable to keep it secure.

### Updating Your Repository

When you make changes to your project, you can update your GitHub repository with:

```bash
git add .
git commit -m "Description of your changes"
git push
```

## Troubleshooting

If you encounter issues with large files, you might need to use Git LFS (Large File Storage). See [Git LFS documentation](https://git-lfs.github.com/) for more information.
