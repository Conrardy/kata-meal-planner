#!/usr/bin/env node

const fs = require('fs');
const path = require('path');

// Project root is parent of scripts directory
const PROJECT_ROOT = path.resolve(__dirname, '..');
const DOCS_READMES_DIR = path.join(PROJECT_ROOT, 'docs', 'readmes');
const EXCLUDED_DIRS = new Set([
  'node_modules',
  'dist',
  'site',
  '.git',
  'coverage',
  '.angular',
]);

/**
 * Check if a path should be excluded
 */
function shouldExclude(filePath) {
  const parts = filePath.split(path.sep);
  return parts.some((part) => EXCLUDED_DIRS.has(part));
}

/**
 * Find all README.md files in the project
 */
function findReadmes(dir) {
  const readmes = [];

  function walk(currentDir) {
    let entries;
    try {
      entries = fs.readdirSync(currentDir, { withFileTypes: true });
    } catch (err) {
      console.error(`Error reading directory ${currentDir}:`, err.message);
      return;
    }

    for (const entry of entries) {
      const fullPath = path.join(currentDir, entry.name);

      if (shouldExclude(fullPath)) {
        continue;
      }

      if (entry.isDirectory()) {
        walk(fullPath);
      } else if (entry.name === 'README.md') {
        readmes.push(fullPath);
      }
    }
  }

  walk(dir);
  return readmes.sort();
}

/**
 * Convert relative path to output filename
 * e.g., frontend/README.md → frontend.md
 * e.g., docs/issues/README.md → docs--issues.md
 */
function getOutputFilename(readmePath) {
  const relativePath = path.relative(PROJECT_ROOT, readmePath);
  const dir = path.dirname(relativePath);

  if (dir === '.') {
    return 'root.md';
  }

  // Replace path separators with double dashes
  return dir.replace(/\\/g, '--').replace(/\//g, '--') + '.md';
}

/**
 * Get the relative path as shown in the original structure
 * e.g., frontend/README.md
 */
function getSourcePath(readmePath) {
  return path.relative(PROJECT_ROOT, readmePath).replace(/\\/g, '/');
}

/**
 * Adjust relative links in markdown content
 */
function adjustRelativeLinks(content, sourcePath) {
  const sourceDir = path.dirname(sourcePath);
  const targetDir = 'docs/readmes';

  // Regex patterns for markdown links: [text](path)
  // Matches both relative and absolute paths
  const linkPattern = /\[([^\]]+)\]\(([^)]+)\)/g;

  return content.replace(linkPattern, (match, text, url) => {
    // Skip URLs with protocol (http://, https://, etc.)
    if (/^https?:\/\//.test(url) || /^#/.test(url) || /^mailto:/.test(url)) {
      return match;
    }

    // Skip absolute paths
    if (path.isAbsolute(url)) {
      return match;
    }

    // Resolve the relative path from the source file's directory
    const resolvedPath = path.normalize(
      path.join(sourceDir, url)
    );

    // Calculate relative path from target directory to resolved path
    const relativePath = path.relative(targetDir, resolvedPath);

    // Convert to forward slashes for markdown
    const adjustedUrl = relativePath.replace(/\\/g, '/');

    return `[${text}](${adjustedUrl})`;
  });
}

/**
 * Create the aggregated markdown file
 */
function createAggregatedFile(sourcePath, outputPath, content) {
  const sourcePathFormatted = getSourcePath(sourcePath);

  // Add source header at the beginning
  const sourceHeader = `> **Source**: \`${sourcePathFormatted}\`\n\n`;

  // Adjust relative links
  const adjustedContent = adjustRelativeLinks(content, sourcePathFormatted);

  const finalContent = sourceHeader + adjustedContent;

  try {
    fs.writeFileSync(outputPath, finalContent, 'utf8');
    console.log(`  ✓ ${sourcePathFormatted} → ${path.basename(outputPath)}`);
  } catch (err) {
    console.error(`  ✗ Failed to write ${outputPath}:`, err.message);
  }
}

/**
 * Generate index.md file with links to all aggregated READMEs
 */
function generateIndex(readmes) {
  const indexContent = `# Aggregated READMEs

This directory contains all README.md files aggregated from the project.
Each file is automatically collected from its source location, with relative links adjusted.

## Index

`;

  const entries = readmes.map((sourcePath) => {
    const outputFilename = getOutputFilename(sourcePath);
    const sourcePathFormatted = getSourcePath(sourcePath);

    // Create a friendly title from the source path
    const title = sourcePathFormatted === 'README.md' ? 'Root README' : sourcePathFormatted;

    return `- [${title}](./${outputFilename})`;
  });

  const memoryBankSection = `

## Reference: Memory Bank

The memory-bank files provide detailed project context and are also integrated in the documentation:

- [Vision & Domaine](../memory-bank/PROJECT_BRIEF.md)
- [Stack Technique](../memory-bank/STACK.md)
- [Structure du Code](../memory-bank/CODEBASE_STRUCTURE.md)
- [Architecture](../memory-bank/common/ARCHITECTURE.md)
- [Standards de Code](../memory-bank/common/CODING_ASSERTIONS.md)
- [Tests](../memory-bank/common/TESTING.md)
- [Conventions Backend](../memory-bank/backend/CONVENTIONS.md)
- [Conventions Frontend](../memory-bank/frontend/CONVENTIONS.md)
- [Design System](../memory-bank/frontend/DESIGN.md)
- [Deployment](../memory-bank/infra/DEPLOYMENT.md)
`;

  const indexPath = path.join(DOCS_READMES_DIR, 'index.md');

  try {
    fs.writeFileSync(indexPath, indexContent + entries.join('\n') + memoryBankSection + '\n', 'utf8');
    console.log(`\n✓ Generated index: docs/readmes/index.md`);
  } catch (err) {
    console.error(`✗ Failed to write index:`, err.message);
  }
}

/**
 * Main execution
 */
function main() {
  console.log('Aggregating README files...\n');

  // Clean directory
  if (fs.existsSync(DOCS_READMES_DIR)) {
    try {
      const files = fs.readdirSync(DOCS_READMES_DIR);
      for (const file of files) {
        fs.unlinkSync(path.join(DOCS_READMES_DIR, file));
      }
      console.log(`Cleaned docs/readmes/ directory\n`);
    } catch (err) {
      console.error(`Error cleaning directory:`, err.message);
    }
  } else {
    try {
      fs.mkdirSync(DOCS_READMES_DIR, { recursive: true });
      console.log(`Created docs/readmes/ directory\n`);
    } catch (err) {
      console.error(`Error creating directory:`, err.message);
      return;
    }
  }

  // Find all README files
  const readmes = findReadmes(PROJECT_ROOT);

  if (readmes.length === 0) {
    console.log('No README files found.');
    return;
  }

  console.log(`Found ${readmes.length} README file(s):\n`);

  // Process each README
  for (const sourcePath of readmes) {
    const outputPath = path.join(DOCS_READMES_DIR, getOutputFilename(sourcePath));

    try {
      const content = fs.readFileSync(sourcePath, 'utf8');
      createAggregatedFile(sourcePath, outputPath, content);
    } catch (err) {
      console.error(
        `  ✗ Failed to read ${getSourcePath(sourcePath)}:`,
        err.message
      );
    }
  }

  // Generate index
  generateIndex(readmes);

  console.log(`\n✓ Aggregation complete!`);
}

main();
