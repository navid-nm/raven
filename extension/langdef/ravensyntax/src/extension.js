const vscode = require("vscode");
const RavenLinter = require("./linter");

// Converts tabs to a fixed number of spaces
function private_convertTabsToSpaces(code, tabSize = 4) {
   const lines = code.split("\n");
   return lines
      .map((line) =>
         line.replace(/^\t+/g, (match) =>
            " ".repeat(match.length * tabSize)
         )
      )
      .join("\n");
}

// Aligns type definitions and normalizes tabs to 4 spaces
function alignDefinitions(code) {
   const lines = code
      .split("\n")
      .map((line) => line.replace(/\n/g, "\\n")); // Escape newlines

   // Extract and process lines for type definitions and other keywords
   const typeDefLines = lines
      .filter(
         (line) =>
            line.trim().startsWith("||") ||
            line.trim().startsWith("xval") ||
            line.trim().startsWith("xlet") ||
            line.trim().startsWith("xvar")
      )
      .map((line) =>
         line.trim().startsWith("||") ||
         line.trim().startsWith("xval") ||
         line.trim().startsWith("xlet") ||
         line.trim().startsWith("xvar")
            ? line
                 .trim()
                 .slice(line.trim().indexOf(" ") + 1)
                 .trim() // Remove the leading `||`, `xval`, `xlet`, or `xvar` and trim whitespace
            : line
      );

   // Find the maximum length of type definitions
   const maxLength = Math.max(
      ...typeDefLines.map((line) =>
         line.indexOf("->") !== -1 ? line.indexOf("->") : 0
      )
   );

   const formattedLines = lines
      .map((line) => {
         if (
            line.trim().startsWith("||") ||
            line.trim().startsWith("xval") ||
            line.trim().startsWith("xlet") ||
            line.trim().startsWith("xvar")
         ) {
            const trimmedLine = line.trim().startsWith("||")
               ? line.trim().slice(2).trim()
               : line
                    .trim()
                    .slice(line.trim().indexOf(" ") + 1)
                    .trim(); // Remove leading keyword
            const index = trimmedLine.indexOf("->");
            if (index !== -1) {
               return (
                  line.slice(0, line.indexOf(trimmedLine.trim())) + // Preserve original prefix
                  trimmedLine.slice(0, index).padEnd(maxLength + 2) +
                  trimmedLine.slice(index)
               );
            }
            return line; // Return line unchanged if no "->" found
         }
         return line;
      })
      .join("\n");

   return private_convertTabsToSpaces(formattedLines);
}

// Helper function to replace text outside string literals and comments
function replaceOutsideStringsAndComments(text, regex, replacement) {
   const stringAndCommentRegex = /(["'`].*?["'`]|\/\/.*?$)/gm;
   let result = "";
   let lastIndex = 0;

   // Find all string literals and comments
   text.replace(stringAndCommentRegex, (match, p1, offset) => {
      // Replace text outside the string literals and comments
      result += text
         .substring(lastIndex, offset)
         .replace(regex, replacement);
      // Append the string literal or comment without changes
      result += match;
      // Update the last processed index
      lastIndex = offset + match.length;
      return match;
   });

   // Replace text after the last string literal or comment
   result += text.substring(lastIndex).replace(regex, replacement);
   return result;
}

// Helper function to handle regex replacements without repeated escaping
function formatRavenDocument(document) {
   let formattedText = document.getText();

   // Define the replacements
   const replacements = [
      { regex: /\bconsole\.log\b/g, replacement: "say" },
      { regex: /\.toString\b/g, replacement: ".str" },
      { regex: /\bconsole\.error\b/g, replacement: "warn" },
      { regex: /\bthis\./g, replacement: "my." },
      { regex: /\bconstructor\b/g, replacement: "init" },
      { regex: /\brequire\b/g, replacement: "use" },
      { regex: /\bfunction\b/g, replacement: "fn" },
      { regex: /\bprivate\b/g, replacement: "closed" },
      { regex: /\bpublic\b/g, replacement: "open" },
      { regex: /\bstatic\b/g, replacement: "stat" },
      { regex: /\bcatch\b/g, replacement: "die" },
      { regex: /\bconst\b/g, replacement: "val" },
      { regex: /\belse\s+if\b/g, replacement: "elif" },
   ];

   // Apply the replacements outside string literals and comments
   replacements.forEach(({ regex, replacement }) => {
      formattedText = replaceOutsideStringsAndComments(
         formattedText,
         regex,
         replacement
      );
   });

   // Replace statements like 'val <module> = use("<module>")' with 'use <module>'
   formattedText = formattedText.replace(
      /\b(?:val|const)\s+(\w+)\s*=\s*use\(["']\1["']\);?/g,
      "use $1"
   );

   formattedText = alignDefinitions(formattedText);
   const fullRange = new vscode.Range(
      document.positionAt(0),
      document.positionAt(document.getText().length)
   );

   return [vscode.TextEdit.replace(fullRange, formattedText)];
}

function activate(context) {
   const linter = new RavenLinter();
   vscode.workspace.onDidOpenTextDocument(linter.lintDocument, linter);
   vscode.workspace.onDidChangeTextDocument((event) =>
      linter.lintDocument(event.document)
   );
   vscode.workspace.onDidSaveTextDocument(linter.lintDocument, linter);

   context.subscriptions.push(
      vscode.languages.registerDocumentFormattingEditProvider("raven", {
         provideDocumentFormattingEdits(document) {
            return formatRavenDocument(document);
         },
      })
   );

   context.subscriptions.push(linter);
}

function deactivate() {}

module.exports = {
   activate,
   deactivate,
};
