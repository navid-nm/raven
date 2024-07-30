grammar Raven;

// Entry point
program: statement*;

// Define possible statements in the program
statement:
	functionDef
	| variableDef
	| importStatement
	| typeHint
	| abbreviation
	| call
	| onReady
	| abbreviationDef
	| expressionStatement
	| asyncFunction
	| errorHandling;

// Function definition
functionDef: 'fn' identifier '(' ')' '{' statement* '}';

// Variable definition
variableDef: 'let' identifier '=' expression ';';

// Import statement
importStatement: 'import' identifier ';';

// Type hint definition
typeHint: '||' identifier '->' identifier;

// Abbreviation definition
abbreviation: 'abbrev' '{' abbreviationDef* '}';

// Abbreviation definition inside an abbrev block
abbreviationDef: identifier '=' identifier;

// Abbreviation usage
abbreviationUsage: '||' identifier '->' identifier;

// Asynchronous function definition
asyncFunction: identifier '(' ')' 'async' '{' statement* '}';

// Call expression
call: identifier '(' (expression (',' expression)*)? ')';

// Error handling
errorHandling:
	'try' '{' statement* '}' ('die' '(' identifier ')')? '{' statement* '}';

// OnReady
onReady: 'onready' '(' 'fn' '(' ')' '{' statement* '}' ')';

// Expression statement
expressionStatement: expression ';';

// Define expressions
expression:
	identifier
	| typeHint
	| call
	| '(' expression ')'
	| expression binaryOp expression;

// Binary operators
binaryOp:
	'+'
	| '-'
	| '*'
	| '/'
	| '=='
	| '!='
	| '<'
	| '>'
	| '<='
	| '>=';

// Identifiers (variable names, function names, etc.)
identifier: '[a-zA-Z_][a-zA-Z0-9_]*';