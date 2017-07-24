# LinSpell
Fast Spelling correction & Approximate string search

The LinSpell spelling correction algorithm does not require edit candidate generation or specialized data structures like BK-tree or Norvig's algorithm. In most cases LinSpell ist faster and requires less memory compared to BK-tree or Norvig's algorithm.
LinSpell is language and character set independent.

<br>

```
Copyright (C) 2017 Wolf Garbe
Version: 1.0
Author: Wolf Garbe <wolf.garbe@faroo.com>
Maintainer: Wolf Garbe <wolf.garbe@faroo.com>
URL: https://github.com/wolfgarbe/linspell
Description:
https://medium.com/@wolfgarbe/symspell-vs-bk-tree-100x-faster-fuzzy-string-search-spell-checking-c4f10d80a078
License:
This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License, 
version 3.0 (LGPL-3.0) as published by the Free Software Foundation.
http://www.opensource.org/licenses/LGPL-3.0
```

#### Usage
single word + Enter:  Display spelling suggestions<br>
Enter without input:  Terminate the program

#### Performance

![Benchmark](https://cdn-images-1.medium.com/max/800/1*1l_5pOYU3AhoijKfVD-Qag.png "Benchmark")
<br>
[Benchmark 1](https://medium.com/@wolfgarbe/symspell-vs-bk-tree-100x-faster-fuzzy-string-search-spell-checking-c4f10d80a078)

#### Applications

* Query correction (10–15% of queries contain misspelled terms),
* Chatbots,
* OCR post-processing,
* Automated proofreading.

#### Frequency dictionary
The [word frequency list](https://github.com/wolfgarbe/symspell/blob/master/wordfrequency_en.txt) was created by intersecting the two lists mentioned below. By reciprocally filtering only those words which appear in both lists are used. Additional filters were applied and the resulting list truncated to &#8776; 80,000 most frequent words.
* [Google Books Ngram data](http://storage.googleapis.com/books/ngrams/books/datasetsv2.html)   [(License)](https://creativecommons.org/licenses/by/3.0/) : Provides representative word frequencies
* [SCOWL - Spell Checker Oriented Word Lists](http://wordlist.aspell.net/)   [(License)](http://wordlist.aspell.net/scowl-readme/) : Ensures genuine English vocabulary    

#### Blog Posts: Algorithm, Benchmarks, Applications
[SymSpell vs. BK-tree: 100x faster fuzzy string search & spell checking](https://medium.com/@wolfgarbe/symspell-vs-bk-tree-100x-faster-fuzzy-string-search-spell-checking-c4f10d80a078)
<br><br>
