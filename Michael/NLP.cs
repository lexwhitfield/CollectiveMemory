using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Michael
{

    public class Corpus
    {
        public List<Paragraph> Paragraphs;
        public List<TwoGram> TwoGrams;
        public List<ThreeGram> ThreeGrams;

        public Corpus()
        {
            this.Paragraphs = new List<Paragraph>();
            this.TwoGrams = new List<TwoGram>();
            this.ThreeGrams = new List<ThreeGram>();
        }

        public Paragraph ProcessParagraph(string p)
        {
            Paragraph paragraph = new Paragraph();
            List<Token> tokens = new List<Token>();

            string[] brokenBySpaces = p.Split(' ');

            foreach (string word in brokenBySpaces)
            {
                tokens.AddRange(ProcessWord(word));
            }

            Sentence currentSentence = new Sentence();
            //loop through tokens adding current collection as sentence when a terminator is reached
            foreach (Token t in tokens)
            {
                //add to current sentence
                currentSentence.Tokens.Add(t);

                //if token is terminator then add as sentence
                if (Form1.terminators.Contains(t.Value))
                {
                    paragraph.Sentences.Add(currentSentence);
                    currentSentence = new Sentence();
                }
            }

            //if last sentence had no terminator catch it here
            if (currentSentence.Tokens.Count > 0)
                paragraph.Sentences.Add(currentSentence);

            //loop sentences and add two- and three-grams

            return paragraph;
        }

        private List<Token> ProcessWord(string wordTemp)
        {
            List<Token> newTokens = new List<Token>();

            if (wordTemp == string.Empty)
                return newTokens;

            //check is punctuation
            if (Form1.punctuation.Contains(wordTemp))
            {
                newTokens.Add(new Token(wordTemp));
                return newTokens;
            }

            //check for leading punctuation
            bool hasLeadingPunctuation = true;
            List<Token> leadingPunctuation = new List<Token>();

            while (hasLeadingPunctuation)
            {
                if (Form1.punctuation.Contains(wordTemp[0].ToString()))
                {
                    leadingPunctuation.Add(new Token(wordTemp[0].ToString()));
                    wordTemp = wordTemp.Remove(0, 1);
                }
                else
                    hasLeadingPunctuation = false;
            }

            //check for tailing punctuation
            bool hasTailingPunctuation = true;
            List<Token> tailingPunctuation = new List<Token>();

            while (hasTailingPunctuation)
            {
                if (Form1.punctuation.Contains(wordTemp[wordTemp.Length - 1].ToString()))
                {
                    tailingPunctuation.Add(new Token(wordTemp[wordTemp.Length - 1].ToString()));
                    wordTemp = wordTemp.Remove(wordTemp.Length - 1, 1);
                }
                else
                    hasTailingPunctuation = false;
            }

            float numericTest = 0.0f;
            if (float.TryParse(wordTemp, out numericTest))
            {
                //word is a valid number
                newTokens.Add(new Token(wordTemp));
                wordTemp = string.Empty;
            }

            //run the word
            //bool containsNumbers = false;
            //bool containsOtherPunctuation = false;
            //bool isValidWord = true;

            //foreach (char letter in wordTemp)
            //{
            //    if (Form1.numbers.Contains(letter.ToString()))
            //    {
            //        containsNumbers = true;
            //        isValidWord = false;
            //    }

            //    if (Form1.punctuation.Contains(letter.ToString()))
            //    {
            //        containsOtherPunctuation = true;
            //        isValidWord = false;
            //    }
            //}

            newTokens.Add(new Token(wordTemp));

            List<Token> returnTokens = new List<Token>();
            returnTokens.AddRange(leadingPunctuation);
            returnTokens.AddRange(newTokens);
            returnTokens.AddRange(tailingPunctuation);

            return returnTokens;
        }
    }

    public class Paragraph
    {
        public List<Sentence> Sentences;

        public Paragraph()
        {
            Sentences = new List<Sentence>();
        }
    }

    public class Sentence
    {
        public List<Token> Tokens;

        public Sentence()
        {
            this.Tokens = new List<Token>();
        }
    }

    public class NounPhrase
    {

    }

    public class VerbPhrase
    {

    }

    public class Token
    {
        public string Value;
        public TokenType Type;

        public Token(string value)
        {
            Value = value;
            Type = TokenType.UNKNOWN;
        }
    }

    public class TwoGram
    {
        public Token Token1;
        public Token Token2;
        public int Frequency;

        public TwoGram(Token t1, Token t2)
        {
            Token1 = t1;
            Token2 = t2;
            Frequency = 1;
        }
    }

    public class ThreeGram
    {
        public Token Token1;
        public Token Token2;
        public Token Token3;
        public int Frequency;

        public ThreeGram(Token t1, Token t2, Token t3)
        {
            Token1 = t1;
            Token2 = t2;
            Token3 = t3;
            Frequency = 1;
        }
    }

    public class FourGram
    {

    }

    public enum TokenType
    {
        UNKNOWN, NOUN, VERB, ARTICLE, ADJECTIVE, PREPOSITION, PRONOUN, ADVERB, CONJUNCTION, INTERJECTION, PUNCTUATION
    }

    public class ControlUtility
    {
        public static List<T> FindControls<T>(Control parent) where T : Control
        {
            List<T> foundControls = new List<T>();

            FindControls<T>(parent, foundControls);

            return foundControls;
        }

        static void FindControls<T>(Control parent, List<T> foundControls) where T : Control
        {
            foreach (Control c in parent.Controls)
            {
                if (c is T)
                    foundControls.Add((T)c);
                else if (c.Controls.Count > 0)
                    FindControls<T>(c, foundControls);
            }
        }

        public static Control FindControl(Control parent, string id)
        {
            foreach (Control c in parent.Controls)
            {
                if (c.Name == id)
                    return c;
                else if (c.Controls.Count > 0)
                {
                    Control foundControl = null;
                    foundControl = FindControl(c, id);
                    if (foundControl != null)
                        return foundControl;
                }
            }

            return null;
        }
    }
}
