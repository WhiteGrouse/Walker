#include <iostream>
#include <string>
#include <vector>
#include <exception>

class TOKEN {};
class NAME : public TOKEN {
public:
	std::string Name;

	NAME(std::string name) : Name(name) {}
};
class SCOPE : public TOKEN {};
class BOUNDARY : public TOKEN {};
class TMP : public TOKEN {
public:
	std::string Str;

	TMP(std::string str) : Str(str) {}
};
class TEMPLATE : public TOKEN {
public:
	std::vector<std::vector<TOKEN>> Args;

	TEMPLATE(std::vector<std::vector<TOKEN>>& args) : Args(args) {}
};
class BRACKET : public TOKEN {
public:
	std::vector<std::vector<TOKEN>> Args;

	BRACKET(std::vector<std::vector<TOKEN>>& args) : Args(args) {}
};
class SQUARE_BRACKET : public TOKEN {
public:
	std::vector<std::vector<TOKEN>> Args;

	SQUARE_BRACKET(std::vector<std::vector<TOKEN>>& args) : Args(args) {}
};

class Parser {
public:
	static std::vector<TOKEN> Parse(std::string demangled) {
		Parser parser(Sanitize(demangled));
		parser._Parse();
		return parser.Tokens;
	}

private:
	Parser(std::string demangled) : Demangled(demangled) {}

	const std::string Demangled;
	int Current = 0;
	std::vector<TOKEN> Tokens;

	static void ReplaceInPlace(std::string& str, const std::string& oldValue, const std::string& newValue) {
		size_t index = 0;
		while ((index = str.find(oldValue, index)) != std::string::npos) {
			str.replace(index, newValue.length(), newValue);
			index += newValue.length();
		}
	}
	static std::string ToHex(const std::string& str) {
		static const char* charset = "0123456789ABCDEF";
		size_t length = str.length();
		std::string result;
		result.reserve(length * 2);
		for (size_t i = 0; i < length; ++i) {
			unsigned char c = str[i];
			result.push_back(charset[c >> 4]);
			result.push_back(charset[c & 15]);
		}
		return result;
	}
	static std::string Sanitize(const std::string& str) {
		std::string result = str;
		ReplaceInPlace(result, "> >", ">>");
		ReplaceInPlace(result, ", ", ",");
		ReplaceInPlace(result, "operator()", "op#2829");
		size_t index = 0;
		while ((index = str.find("operator", index)) != std::string::npos) {
			std::string op = str.substr(index + 8, str.find("(", index) - index - 8);
			ReplaceInPlace(result, "operator" + op, "op#" + ToHex(op));
		}
		return result;
	}

	void _Parse() {
		static const std::string NAME_CHARSET = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_~#*&.-+:";
		static const std::string start = "<([{";
		std::string buf;
		for (; Current < Demangled.length(); ++Current) {
			if (start.find(Demangled[Current]) != std::string::npos) {
				if (buf.length() > 0) {
					Tokens.push_back(NAME(buf));
					buf = "";
				}
				if (Demangled[Current] == '<') {
					std::vector<std::vector<TOKEN>> args;
					for (auto& token : _Split(_Cut())) {
						args.push_back(Parse(token));
					}
					Tokens.push_back(TEMPLATE(args));
				}
				else if (Demangled[Current] == '(') {
					std::vector<std::vector<TOKEN>> args;
					for (auto& token : _Split(_Cut())) {
						args.push_back(Parse(token));
					}
					Tokens.push_back(BRACKET(args));
				}
				else if (Demangled[Current] == '[') {
					std::vector<std::vector<TOKEN>> args;
					for (auto& token : _Split(_Cut())) {
						args.push_back(Parse(token));
					}
					Tokens.push_back(SQUARE_BRACKET(args));
				}
				else if (Demangled[Current] == '{') {
					Tokens.push_back(TMP(_Cut()));
				}
			}
			else if (_CheckToken("::")) {
				if (buf.length() > 0) {
					Tokens.push_back(NAME(buf));
					buf = "";
				}
				Tokens.push_back(SCOPE());
				++Current;
			}
			else if (NAME_CHARSET.find(Demangled[Current]) == std::string::npos) {
				if (buf.length() > 0) {
					Tokens.push_back(NAME(buf));
					buf = "";
				}
				Tokens.push_back(BOUNDARY());
			}
			else {
				buf += Demangled[Current];
			}
		}
		if (buf.length() > 0) {
			Tokens.push_back(NAME(buf));
		}
	}

	bool _CheckToken(const std::string& str) {
		if (Current + str.length() > Demangled.length())
			return false;
		for (int i = 0; i < str.length(); ++i) {
			if (str[i] != Demangled[Current + i]) {
				return false;
			}
		}
		return true;
	}

	std::string _Cut() {
		static const std::string start = "<([{";
		static const std::string end = ">)]}";
		int nest = 0;
		int offset = Current;
		if (start.find(Demangled[Current]) == std::string::npos)
			throw std::invalid_argument("The first character is not a nesting start symbol");
		for (; Current < Demangled.length(); ++Current) {
			if (start.find(Demangled[Current]) != std::string::npos)
				++nest;
			else if (end.find(Demangled[Current]) != std::string::npos) {
				if (--nest == 0) {
					break;
				}
			}
		}
		if (nest > 0 || end.find(Demangled[Current]) == std::string::npos)
			throw std::invalid_argument("Nest is not closed properly");
		return Demangled.substr(offset + 1, Current - offset - 1);
	}

	std::vector<std::string> _Split(const std::string& target) {
		static const std::string start = "<([{";
		static const std::string end = ">)]}";
		int nest = 0;
		int offset = 0;
		std::vector<std::string> list;
		for (int i = 0; i < target.length(); ++i) {
			if (start.find(target[i]) != std::string::npos)
				++nest;
			else if (end.find(target[i]) != std::string::npos)
				--nest;
			else if (target[i] == ',' && nest == 0) {
				list.push_back(target.substr(offset, i - offset));
				offset = i + 1;
			}
		}
		if (nest > 0)
			throw std::invalid_argument("Nest is not closed properly");
		if (offset < target.length() - 1)
			list.push_back(target.substr(offset));
		return list;
	}
};

int main(void) {
	std::string symbol;
	std::cout << "> ";
	std::getline(std::cin, symbol);
	auto tokens = Parser::Parse(symbol);

	return 0;
}