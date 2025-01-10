This is a Windows App/Program which re-creates Galaxy News Radio, complete with Three Dog's witty comments. It allows you to select which quests you have completed, what the outcome was, and just like in-game Three Dog will make the related comments during the commercial break. 
This makes no changes to Fallout 3, it is a stand-alone program for any Windows Computer. This allows for listening to GNR without having to boot up Fallout 3, as well as having whatever Player Stats and Quest Outcomes you'd like.

Welcome to the brand new GNR 2.0!! (05/24/23)
Completely remade from the ground up with cleaner coding, more of ThreeDog's commentary, and now running using a built-in version of the Windows Media Player, meaning that all audio should be perfectly timed and in sync, while also allowing you to pause and resume audio playback without losing your place in the program.

I had originally written in the use of the WinMediaPlayer... Probably a few years ago at this point. I believe I had just finished integrating it when coincidentally someone sent me a message asking if I'd ever update the app. Unfortunately, I had gotten too ambitious and scatter brained, leaving me with a lot of "I'll use it in the future" code in the program, along with a bunch of extra code due to not really knowing what I was doing.

It was a few months ago when suddenly my ADHD muse decided I needed to come back to this, after using Visual Studio for a few in-house projects at work (God I was barely out of high school when I first made this, if that). I could barely make sense of the jumble of spaghetti code I had back then even if it somehow kinda worked. I am also a major victim of the 'Copy something to a USB', 'Oh now I need it for something else, rip everything off into a folder somewhere' mindset, leaving me with probably 4-6 copies of the original project and no real idea which was the furthest along.

Anyway, enough rambling. On to the big changes
As I mentioned, this now uses the Windows Media Player for audio playback. I think that limits its use to Windows PCs.
Also instead of an installer program, its now just a folder of loose files and the .exe (Mainly because the knowledge of how I made the installer is now lost). This may require you to install some version of an MS .NET re-distributable, but I'm not entirely sure. Your PC should prompt you if it needs it with a MS link.
Also, some of the audio files relating to Broken Steel content are not yet included, along with some player audio.
I have also re-introduced song specific intro and outro voice lines for ThreeDog (ex. That was/Here is Billie Holiday with Easy Living).
As of v2.1, the Save/Load system has been reintroduced. This time however it runs off of Windows Explorer like a civilized program; no more having to remember obscure file names to save and load settings.
