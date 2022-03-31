# Project Description
### THIS IS A PROJECT RECOVERED FROM THE WAYBACK MACHINE, SINCE THE ORIGINAL SOURCE REPOSITORY, HOSTED AT CODEPLEX.COM, IS LOST.


Converts subtitles from DVDs and PGS (Bluray .sup) files into Advanced Substation Alpha and SRT text format using OCR (optical character recognition).

I've created this app to extract subtitles from (unencrypted, or on hard drive) DVDs and convert them to Advanced Substation Alpha or SRT text format. It can also convert individual sup (PGS) and sub/idx files. I wrote this because I hate the blocky, too-high-on-the-screen look of regular DVD subtitles and wanted to re-encode my DVD collection in h264/aac/ssa in an mkv container.

It's a wizard-style app, allowing you to pick program chains, angles, audio and subtitle tracks from a DVD folder and create mpg, d2v and bin (my own data format similar to sub/idx) files for each. DGIndex is used to help line up the subs to the video since DVD programs often have discontinuities that mess up sync. If you already have the subtitle track in a separate file(s), you can skip ahead and begin at the OCR step of the wizard.

The OCR is basic but accurate: exact pattern matching of the characters for DVDs, with some fuzzy logic added for High Definition subtitles. The starting OCR database is quite large so most DVDs should require manual matching of just a few characters. Some characters like  l, I, 1, o must be manually matched for every DVD since they have a lot of false positives.

The line and word layout functions are pretty sophisticated and should give good results unless the characters are very unusual (vertical or upside-down text is bad).

Quite a few features have been added to support accents and other non-English character sets, including Thai, but the program itself is not translated beyond English yet.
