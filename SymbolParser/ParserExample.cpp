#include <iostream>
#include <sstream>
#include <string>
#include <vector>
#include <exception>

enum TOKEN_TYPE {
	Name,
	Scope,
	Boundary,
	Temporary,
	Template,
	Bracket,
	SquareBracket
};
class TOKEN {
public:
	virtual ~TOKEN() = default;

	virtual TOKEN_TYPE GetType() = 0;
};
class NAME : public TOKEN {
public:
	std::string Name;

	NAME(std::string name) : Name(name) {}

	TOKEN_TYPE GetType() override {
		return TOKEN_TYPE::Name;
	}
};
class SCOPE : public TOKEN {
public:
	TOKEN_TYPE GetType() override {
		return TOKEN_TYPE::Scope;
	}
};
class BOUNDARY : public TOKEN {
public:
	TOKEN_TYPE GetType() override {
		return TOKEN_TYPE::Boundary;
	}
};
class TMP : public TOKEN {
public:
	std::string Str;

	TMP(std::string str) : Str(str) {}

	TOKEN_TYPE GetType() override {
		return TOKEN_TYPE::Temporary;
	}
};
class TOKEN_HasArgs : public TOKEN {
public:
	std::vector<std::vector<TOKEN*>> Args;

	TOKEN_HasArgs(std::vector<std::vector<TOKEN*>>& args) : Args(args) {}

	virtual ~TOKEN_HasArgs() {
		for (auto& tokens : Args) {
			for (int i = 0; i < tokens.size(); ++i) {
				delete tokens[i];
			}
		}
	}
};
class TEMPLATE : public TOKEN_HasArgs {
public:
	TEMPLATE(std::vector<std::vector<TOKEN*>>& args) : TOKEN_HasArgs(args) {}

	TOKEN_TYPE GetType() override {
		return TOKEN_TYPE::Template;
	}
};
class BRACKET : public TOKEN_HasArgs {
public:
	BRACKET(std::vector<std::vector<TOKEN*>>& args) : TOKEN_HasArgs(args) {}

	TOKEN_TYPE GetType() override {
		return TOKEN_TYPE::Bracket;
	}
};
class SQUARE_BRACKET : public TOKEN_HasArgs {
public:
	SQUARE_BRACKET(std::vector<std::vector<TOKEN*>>& args) : TOKEN_HasArgs(args) {}

	TOKEN_TYPE GetType() override {
		return TOKEN_TYPE::SquareBracket;
	}
};

class Parser {
public:
	static std::vector<TOKEN*> Parse(std::string& demangled) {
		Parser parser(Sanitize(demangled));
		parser._Parse();
		return parser.Tokens;
	}

private:
	Parser(std::string demangled) : Demangled(demangled) {}

	const std::string Demangled;
	int Current = 0;
	std::vector<TOKEN*> Tokens;

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
					Tokens.push_back(new NAME(buf));
					buf = "";
				}
				if (Demangled[Current] == '<') {
					std::vector<std::vector<TOKEN*>> args;
					for (auto& token : _Split(_Cut())) {
						args.push_back(Parse(token));
					}
					Tokens.push_back(new TEMPLATE(args));
				}
				else if (Demangled[Current] == '(') {
					std::vector<std::vector<TOKEN*>> args;
					for (auto& token : _Split(_Cut())) {
						args.push_back(Parse(token));
					}
					Tokens.push_back(new BRACKET(args));
				}
				else if (Demangled[Current] == '[') {
					std::vector<std::vector<TOKEN*>> args;
					for (auto& token : _Split(_Cut())) {
						args.push_back(Parse(token));
					}
					Tokens.push_back(new SQUARE_BRACKET(args));
				}
				else if (Demangled[Current] == '{') {
					Tokens.push_back(new TMP(_Cut()));
				}
			}
			else if (_CheckToken("::")) {
				if (buf.length() > 0) {
					Tokens.push_back(new NAME(buf));
					buf = "";
				}
				Tokens.push_back(new SCOPE());
				++Current;
			}
			else if (NAME_CHARSET.find(Demangled[Current]) == std::string::npos) {
				if (buf.length() > 0) {
					Tokens.push_back(new NAME(buf));
					buf = "";
				}
				Tokens.push_back(new BOUNDARY());
			}
			else {
				buf += Demangled[Current];
			}
		}
		if (buf.length() > 0) {
			Tokens.push_back(new NAME(buf));
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

std::string serialize(const std::vector<TOKEN*>& tokens) {
	std::ostringstream str;
	for (int index = 0; index < tokens.size(); ++index) {
		TOKEN* token = tokens[index];
		if (token->GetType() == TOKEN_TYPE::Name) {
			str << "@" << index << ">NAME<" << ((NAME*)token)->Name << ">";
		}
		else if (token->GetType() == TOKEN_TYPE::Scope) {
			str << "@" << index << ">SCOPE";
		}
		else if (token->GetType() == TOKEN_TYPE::Boundary) {
			str << "@" << index << ">BOUNDARY";
		}
		else if (token->GetType() == TOKEN_TYPE::Template) {
			str << "@" << index << ">TEMPLATE";
		}
		else if (token->GetType() == TOKEN_TYPE::Bracket) {
			str << "@" << index << ">BRACKET";
		}
		else if (token->GetType() == TOKEN_TYPE::SquareBracket) {
			str << "@" << index << ">SQUARE_BRACKET";
		}
		else if (token->GetType() == TOKEN_TYPE::Temporary) {
			str << "@" << index << ">TMP";
		}
	}
	return str.str();
}

std::vector<TOKEN*> deserialize(const std::vector<TOKEN*>& tokens, const std::string& serialized) {
	std::vector<TOKEN*> list;
	bool read = false;
	std::string num;
	for (auto& c : serialized) {
		if (read) {
			if ('0' <= c && c <= '9') {
				num += c;
			}
			else {
				list.push_back(tokens[std::stoi(num)]);
				read = false;
				num.clear();
			}
		}
		if (c == '@') {
			read = true;
		}
	}
	return list;
}

int main(void) {
	std::string symbol;
	while (true) {
		std::cout << "> ";
		std::getline(std::cin, symbol);
		auto tokens = Parser::Parse(symbol);
		std::cout << serialize(tokens) << std::endl << std::endl;
		auto deserialized = deserialize(tokens, serialize(tokens));

		for (auto* token : tokens) {
			delete token;
		}
	}

	return 0;
}