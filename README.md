# SmallHax.MangaReader
Minimalistic CBZ manga/comic book reader.

Having run into UI problems developing Rikaikyun2, I've decided to practice on a simpler project.
Since I wasn't sure I can trust apps I used for manga in the past (some possible maleware), I've decided to write my own.

The plan is to have some basic features and keep references as vanilla as possible.

Features:

- Read ZIP and CBZ files
- Reading from left to right and right to left
- Autoscaling for typical scenarios (fit width, height)
- Manual zooming
- Displaying progress information

I might try to add the following later:

- Double page support
- Reading images from a folder
- Reading other file formats
- Correcting page order (for files named without zero padding)
- Correcting contrast
- Some history or at least ability to continue reading from previous instance of application
- Page carousel for navigation

Since this is a MAUI application, it should work on Windows and Android. Other platforms should also be possible. I should be able to prepare a Linux version as well.