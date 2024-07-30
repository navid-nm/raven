// Generated from a:/Projects/Apps/Raven/grammar/antlr/Raven.g4 by ANTLR 4.13.1
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.misc.*;
import org.antlr.v4.runtime.tree.*;
import java.util.List;
import java.util.Iterator;
import java.util.ArrayList;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast", "CheckReturnValue"})
public class RavenParser extends Parser {
	static { RuntimeMetaData.checkVersion("4.13.1", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, T__12=13, T__13=14;
	public static final int
		RULE_program = 0, RULE_statement = 1, RULE_identifier = 2;
	private static String[] makeRuleNames() {
		return new String[] {
			"program", "statement", "identifier"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'fn'", "'('", "')'", "'{'", "'}'", "'let'", "'='", "'0'", "';'", 
			"'import'", "'onready'", "'||'", "'->'", "'[a-zA-Z_][a-zA-Z0-9_]*'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}

	@Override
	public String getGrammarFileName() { return "Raven.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public ATN getATN() { return _ATN; }

	public RavenParser(TokenStream input) {
		super(input);
		_interp = new ParserATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@SuppressWarnings("CheckReturnValue")
	public static class ProgramContext extends ParserRuleContext {
		public List<StatementContext> statement() {
			return getRuleContexts(StatementContext.class);
		}
		public StatementContext statement(int i) {
			return getRuleContext(StatementContext.class,i);
		}
		public ProgramContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_program; }
	}

	public final ProgramContext program() throws RecognitionException {
		ProgramContext _localctx = new ProgramContext(_ctx, getState());
		enterRule(_localctx, 0, RULE_program);
		int _la;
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(9);
			_errHandler.sync(this);
			_la = _input.LA(1);
			while ((((_la) & ~0x3f) == 0 && ((1L << _la) & 7234L) != 0)) {
				{
				{
				setState(6);
				statement();
				}
				}
				setState(11);
				_errHandler.sync(this);
				_la = _input.LA(1);
			}
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class StatementContext extends ParserRuleContext {
		public List<IdentifierContext> identifier() {
			return getRuleContexts(IdentifierContext.class);
		}
		public IdentifierContext identifier(int i) {
			return getRuleContext(IdentifierContext.class,i);
		}
		public StatementContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_statement; }
	}

	public final StatementContext statement() throws RecognitionException {
		StatementContext _localctx = new StatementContext(_ctx, getState());
		enterRule(_localctx, 2, RULE_statement);
		try {
			setState(41);
			_errHandler.sync(this);
			switch (_input.LA(1)) {
			case T__0:
				enterOuterAlt(_localctx, 1);
				{
				setState(12);
				match(T__0);
				setState(13);
				identifier();
				setState(14);
				match(T__1);
				setState(15);
				match(T__2);
				setState(16);
				match(T__3);
				setState(17);
				match(T__4);
				}
				break;
			case T__5:
				enterOuterAlt(_localctx, 2);
				{
				setState(19);
				match(T__5);
				setState(20);
				identifier();
				setState(21);
				match(T__6);
				setState(22);
				match(T__7);
				setState(23);
				match(T__8);
				}
				break;
			case T__9:
				enterOuterAlt(_localctx, 3);
				{
				setState(25);
				match(T__9);
				setState(26);
				identifier();
				setState(27);
				match(T__8);
				}
				break;
			case T__10:
				enterOuterAlt(_localctx, 4);
				{
				setState(29);
				match(T__10);
				setState(30);
				match(T__1);
				setState(31);
				match(T__0);
				setState(32);
				match(T__1);
				setState(33);
				match(T__2);
				setState(34);
				match(T__3);
				setState(35);
				match(T__4);
				}
				break;
			case T__11:
				enterOuterAlt(_localctx, 5);
				{
				setState(36);
				match(T__11);
				setState(37);
				identifier();
				setState(38);
				match(T__12);
				setState(39);
				identifier();
				}
				break;
			default:
				throw new NoViableAltException(this);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	@SuppressWarnings("CheckReturnValue")
	public static class IdentifierContext extends ParserRuleContext {
		public IdentifierContext(ParserRuleContext parent, int invokingState) {
			super(parent, invokingState);
		}
		@Override public int getRuleIndex() { return RULE_identifier; }
	}

	public final IdentifierContext identifier() throws RecognitionException {
		IdentifierContext _localctx = new IdentifierContext(_ctx, getState());
		enterRule(_localctx, 4, RULE_identifier);
		try {
			enterOuterAlt(_localctx, 1);
			{
			setState(43);
			match(T__13);
			}
		}
		catch (RecognitionException re) {
			_localctx.exception = re;
			_errHandler.reportError(this, re);
			_errHandler.recover(this, re);
		}
		finally {
			exitRule();
		}
		return _localctx;
	}

	public static final String _serializedATN =
		"\u0004\u0001\u000e.\u0002\u0000\u0007\u0000\u0002\u0001\u0007\u0001\u0002"+
		"\u0002\u0007\u0002\u0001\u0000\u0005\u0000\b\b\u0000\n\u0000\f\u0000\u000b"+
		"\t\u0000\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001"+
		"\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0001\u0003"+
		"\u0001*\b\u0001\u0001\u0002\u0001\u0002\u0001\u0002\u0000\u0000\u0003"+
		"\u0000\u0002\u0004\u0000\u0000/\u0000\t\u0001\u0000\u0000\u0000\u0002"+
		")\u0001\u0000\u0000\u0000\u0004+\u0001\u0000\u0000\u0000\u0006\b\u0003"+
		"\u0002\u0001\u0000\u0007\u0006\u0001\u0000\u0000\u0000\b\u000b\u0001\u0000"+
		"\u0000\u0000\t\u0007\u0001\u0000\u0000\u0000\t\n\u0001\u0000\u0000\u0000"+
		"\n\u0001\u0001\u0000\u0000\u0000\u000b\t\u0001\u0000\u0000\u0000\f\r\u0005"+
		"\u0001\u0000\u0000\r\u000e\u0003\u0004\u0002\u0000\u000e\u000f\u0005\u0002"+
		"\u0000\u0000\u000f\u0010\u0005\u0003\u0000\u0000\u0010\u0011\u0005\u0004"+
		"\u0000\u0000\u0011\u0012\u0005\u0005\u0000\u0000\u0012*\u0001\u0000\u0000"+
		"\u0000\u0013\u0014\u0005\u0006\u0000\u0000\u0014\u0015\u0003\u0004\u0002"+
		"\u0000\u0015\u0016\u0005\u0007\u0000\u0000\u0016\u0017\u0005\b\u0000\u0000"+
		"\u0017\u0018\u0005\t\u0000\u0000\u0018*\u0001\u0000\u0000\u0000\u0019"+
		"\u001a\u0005\n\u0000\u0000\u001a\u001b\u0003\u0004\u0002\u0000\u001b\u001c"+
		"\u0005\t\u0000\u0000\u001c*\u0001\u0000\u0000\u0000\u001d\u001e\u0005"+
		"\u000b\u0000\u0000\u001e\u001f\u0005\u0002\u0000\u0000\u001f \u0005\u0001"+
		"\u0000\u0000 !\u0005\u0002\u0000\u0000!\"\u0005\u0003\u0000\u0000\"#\u0005"+
		"\u0004\u0000\u0000#*\u0005\u0005\u0000\u0000$%\u0005\f\u0000\u0000%&\u0003"+
		"\u0004\u0002\u0000&\'\u0005\r\u0000\u0000\'(\u0003\u0004\u0002\u0000("+
		"*\u0001\u0000\u0000\u0000)\f\u0001\u0000\u0000\u0000)\u0013\u0001\u0000"+
		"\u0000\u0000)\u0019\u0001\u0000\u0000\u0000)\u001d\u0001\u0000\u0000\u0000"+
		")$\u0001\u0000\u0000\u0000*\u0003\u0001\u0000\u0000\u0000+,\u0005\u000e"+
		"\u0000\u0000,\u0005\u0001\u0000\u0000\u0000\u0002\t)";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}