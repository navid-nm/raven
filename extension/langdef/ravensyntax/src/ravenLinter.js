const vscode = require("vscode");

class RavenLinter {
   constructor() {
      this.diagnostics =
         vscode.languages.createDiagnosticCollection("raven");
   }

   lintDocument(document) {
      if (document.languageId !== "raven") {
         return;
      }

      let diagnostics = [];
      const text = document.getText();
      const lines = text.split("\n");

      lines.forEach((line, i) => {
         if (line.includes("console.log")) {
            const index = line.indexOf("console.log");
            diagnostics.push(
               new vscode.Diagnostic(
                  new vscode.Range(
                     new vscode.Position(i, index),
                     new vscode.Position(i, index + 3)
                  ),
                  "Use say instead of console.log ",
                  vscode.DiagnosticSeverity.Warning
               )
            );
         }
         if (line.includes("catch")) {
            const index = line.indexOf("catch");
            diagnostics.push(
               new vscode.Diagnostic(
                  new vscode.Range(
                     new vscode.Position(i, index),
                     new vscode.Position(i, index + 3)
                  ),
                  "Use die instead of catch",
                  vscode.DiagnosticSeverity.Warning
               )
            );
         }
         if (line.includes("const")) {
            const index = line.indexOf("const");
            diagnostics.push(
               new vscode.Diagnostic(
                  new vscode.Range(
                     new vscode.Position(i, index),
                     new vscode.Position(i, index + 3)
                  ),
                  "Use val instead of const",
                  vscode.DiagnosticSeverity.Warning
               )
            );
         }
      });

      this.diagnostics.set(document.uri, diagnostics);
   }

   dispose() {
      this.diagnostics.dispose();
   }
}

module.exports = RavenLinter;
