A couple of years ago, moola.com debuted - it was a game site where players can play for free and win real money.  Each user of the platform was given a free penny, and they could play double-or-nothing games against other players.  You double that penny enough, and you've got some real money.  The site was supported by advertisements - before each game the players had to watch an ad video.  If a user lost all their money, they could request another free penny.  Genious!

At the time, there was 1 game, the 2-player Gold Rush.  Each player had 6 rocks labeled 1, 2, 3, 4, 5, and 6.  With each turn, each player would choose a rock, hidden from the other player, and then whichever player chose the rock with the higher value would win both.  The rocks selected are then removed from play.  For example, if player A chose rock 3 and player B chose rock 6, then player B would win both rocks, earning 3+6=9 points.  Play continued until all rocks were exhausted and the winner was the player with the most points.

After playing this a lot, I realized there was a way to gain competitive advantage.  In certain situations, there was a deterministic way to guarantee a win, in some cases as early as the 3rd round.  Further, back in 2006 when this came out, there was some user-identifying information provided about your opponent, and users tended to play with similar patterns from game to game.  If you combine the deterministic logic with a probabilistic set of early guesses, you can gain a solid competitive advantage.

So, I coded up a program that uses the minimax algorithm to provide an optimal rock selection with each turn; and a data tracker that would provide probabilistic guesses on what a given opponent might select based on the opponent's name, platform join date, and current game state.  I cashed out at a few hundred bucks.