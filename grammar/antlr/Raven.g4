grammar Raven;

program: statement*;

statement:
	'fn' identifier '(' ')' '{' '}'
	| 'let' identifier '=' '0' ';'
	| 'import' identifier ';'
	| 'onready' '(' 'fn' '(' ')' '{' '}'
	| '||' identifier '->' identifier;

identifier: '[a-zA-Z_][a-zA-Z0-9_]*';