// LinSpell: Spelling correction & Approximate string search
//
// The LinSpell spelling correction algorithm does not require edit candidate generation or specialized data structures like BK-tree or Norvig's algorithm. 
// In most cases LinSpell ist faster and requires less memory compared to BK-tree or Norvig's algorithm.
// LinSpell is language and character set independent.
//
// Copyright (C) 2017 Wolf Garbe
// Version: 1.0
// Author: Wolf Garbe <wolf.garbe@faroo.com>
// Maintainer: Wolf Garbe <wolf.garbe@faroo.com>
// URL: https://github.com/wolfgarbe/linspell
// Description: https://medium.com/@wolfgarbe/symspell-vs-bk-tree-100x-faster-fuzzy-string-search-spell-checking-c4f10d80a078
//
// License:
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License, 
// version 3.0 (LGPL-3.0) as published by the Free Software Foundation.
// http://www.opensource.org/licenses/LGPL-3.0

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

using System.Collections;
using System.Collections.Specialized;

public static class LinSpell
{
    public static int editDistanceMax=2;

    public static int verbose = 0;
    //0: top suggestion
    //1: all suggestions of smallest edit distance   
    //2: all suggestions <= editDistanceMax (slower, no early termination)
    
    public class SuggestItem
    {
        public string term = "";
        public int distance = 0;
        public Int64 count = 0;

        public override bool Equals(object obj)
        {
            return Equals(term, ((SuggestItem)obj).term);
        }
     
        public override int GetHashCode()
        {
            return term.GetHashCode();
        }
    }

    //create a non-unique wordlist from sample text
    //language independent (e.g. works with Chinese characters)
    private static string[] ParseWords(string text)
    {
        // \w Alphanumeric characters (including non-latin characters, umlaut characters and digits) plus "_" 
        // \d Digits
        // Compatible with non-latin characters, does not split words at apostrophes
        MatchCollection mc = Regex.Matches(text.ToLower(), @"['’\w-[_]]+");

        //for benchmarking only: with CreateDictionary("big.txt","") and the text corpus from http://norvig.com/big.txt  the Regex below provides the exact same number of dictionary items as Norvigs regex "[a-z]+" (which splits words at apostrophes & incompatible with non-latin characters)     
        //MatchCollection mc = Regex.Matches(text.ToLower(), @"[\w-[\d_]]+");

        var matches = new string[mc.Count];
        for (int i = 0; i < matches.Length; i++) matches[i] = mc[i].ToString();
        return matches;     
    }
    
    public static int maxlength = 0;//maximum dictionary term length

    //load a frequency dictionary
    public static bool LoadDictionary(string corpus, string language, int termIndex, int countIndex)
    {
        if (!File.Exists(corpus)) return false;
        
        using (StreamReader sr = new StreamReader(File.OpenRead(corpus)))
        {
            String line;

            //process a single line at a time only for memory efficiency
            while ((line = sr.ReadLine()) != null)
            {              
                string[] lineParts = line.Split(null);
                if (lineParts.Length>=2)
                {           
                    string key = lineParts[termIndex];
                    //Int64 count;
                    if (Int64.TryParse(lineParts[countIndex], out Int64 count))
                    {
                        dictionaryLinear.Add(language + key, Math.Min(Int64.MaxValue, count));
                    }
                }
            }

           
        }

        return true;
    }

    //create a frequency dictionary from a corpus
    public static bool CreateDictionary(string corpus, string language)
    {
        if (!File.Exists(corpus)) return false;
        
        using (StreamReader sr = new StreamReader(File.OpenRead(corpus)))
        {
            String line;
            //process a single line at a time only for memory efficiency
            while ((line = sr.ReadLine()) != null)
            {
                foreach (string key in ParseWords(line))
                {
                    if (dictionaryLinear.TryGetValue(language + key, out long value)){ if (value<Int64.MaxValue) dictionaryLinear[language + key]=value+1; } else dictionaryLinear.Add(language + key, 1);
                }
            }
        }

        return true;
    }

    public static Dictionary<string, long> dictionaryLinear = new Dictionary<string, long>();

    //linear search will be O(n), but with a few tweaks it will be almost always faster than a BK-tree. Please mind the constants!
    public static List<SuggestItem> LookupLinear(string input, string language, int editDistanceMax)
    {
        List<SuggestItem> suggestions = new List<SuggestItem>();

        int editDistanceMax2 = editDistanceMax;

        //probably most lookups will be matches, lets get them straight O(1) from a hash table
        if ((verbose < 2) && dictionaryLinear.TryGetValue(language + input, out long value))
        {
            SuggestItem si = new SuggestItem()
            {
                term = input,
                count = value,
                distance = 0
            };
            suggestions.Add(si);

            return suggestions;
        }

        foreach (KeyValuePair<string, long> kv in dictionaryLinear)
        {
            //umdrehen: suche dictionary in input liste

            if (Math.Abs(kv.Key.Length - input.Length) > editDistanceMax2) continue;

            //if already ed1 suggestion, there can be no better suggestion with smaller count: no need to calculate damlev
            if ((verbose == 0) && (suggestions.Count > 0) && (suggestions[0].distance == 1) && (kv.Value <= suggestions[0].count)) continue; //test


            int distance = EditDistance.DamerauLevenshteinDistance(input, kv.Key, editDistanceMax2); 
            //es wird trotz editDistanceMax in levensthein manchmal eine höhere editdistance ausgerechnet
            if ((distance >= 0) && (distance <= editDistanceMax))
            {
                //v0: clear if better ed or better ed+count; 
                //v1: clear if better ed                    
                //v2: all

                //do not process higher distances than those already found, if verbose<2
                if ((verbose < 2) && (suggestions.Count > 0) && (distance > suggestions[0].distance)) continue;

                //we will calculate DamLev distance only to the smallest found distance sof far
                if (verbose < 2) editDistanceMax2 = distance;

                //remove all existing suggestions of higher distance, if verbose<2
                if ((verbose < 2) && (suggestions.Count > 0) && (suggestions[0].distance > distance)) suggestions.Clear();
                SuggestItem si = new SuggestItem()
                {
                    term = kv.Key,
                    count = kv.Value,
                    distance = distance
                };
                suggestions.Add(si);
            }

        }

        //sort by ascending edit distance, then by descending word frequency
        sort: if (verbose < 2) suggestions.Sort((x, y) => -x.count.CompareTo(y.count)); else suggestions.Sort((x, y) => 2 * x.distance.CompareTo(y.distance) - x.count.CompareTo(y.count));
        if ((verbose == 0) && (suggestions.Count > 1)) return suggestions.GetRange(0, 1); else return suggestions;
    }

  

}
