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
      // const text = document.getText();
      // const lines = text.split("\n");
      this.diagnostics.set(document.uri, diagnostics);
   }

   dispose() {
      this.diagnostics.dispose();
   }
}

module.exports = RavenLinter;
