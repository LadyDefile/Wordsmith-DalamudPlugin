﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wordsmith.Helpers;
internal class StatisticsTracker
{
    private Dictionary<string, int> _word_usage_count = new();

    internal void TallyWord( string line, Word word)
    {
        if ( word.WordLength == 0 )
        {
            Wordsmith.PluginLog.Error( $"Word has a length of zero." );
            return;
        }
        string w = line[word.WordIndex..word.WordEndIndex];
        if ( !this._word_usage_count.ContainsKey( w.ToLower() ) )
            this._word_usage_count[w.ToLower()] = 1;
        else
            this._word_usage_count[w.ToLower()]++;
    }

    internal void TallyWords( string line, IEnumerable<Word> words)
    {
        foreach ( Word word in words )
            TallyWord( line, word );
    }

    internal void AddChunk( TextChunk textChunk )
    {
        // Get all of the words in the chunk.
        List<Word> words = textChunk.Text.Words();
        TallyWords( textChunk.Text, words );
    }

    internal List<KeyValuePair<string, int>> ListWordsByCount(bool descending)
    {
        List<KeyValuePair<string, int>> results = new();
        results.AddRange( this._word_usage_count.ToArray() );

        if ( descending )
            return results.OrderByDescending( f => f.Value ).ToList();
        else
            return results.OrderBy( f => f.Value ).ToList();
    }

    internal List<KeyValuePair<string,int>> ListWordsByWord(bool descending)
    {
        List<KeyValuePair<string, int>> results = new();
        results.AddRange( this._word_usage_count.ToArray() );

        if ( descending )
            return results.OrderByDescending( f => f.Key ).ToList();
        else
            return results.OrderBy( f => f.Key ).ToList();
    }

    internal void Clear() => this._word_usage_count.Clear();
}
