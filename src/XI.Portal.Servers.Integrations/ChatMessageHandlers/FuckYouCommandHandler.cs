using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XI.CommonTypes;
using XI.Portal.Servers.Interfaces;
using XI.Servers.Interfaces;

namespace XI.Portal.Servers.Integrations.ChatMessageHandlers
{
    public class FuckYouCommandHandler : ChatCommandHandlerBase
    {
        private readonly IGameServersRepository _gameServersRepository;
        private readonly IGameServerStatusRepository _gameServerStatusRepository;
        private readonly ILogger<FuckYouCommandHandler> _logger;
        private readonly IRconClientFactory _rconClientFactory;

        public FuckYouCommandHandler(
            ILogger<FuckYouCommandHandler> logger,
            IGameServersRepository gameServersRepository,
            IGameServerStatusRepository gameServerStatusRepository,
            IRconClientFactory rconClientFactory) : base(new []{ "!fu" })
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _gameServersRepository = gameServersRepository ?? throw new ArgumentNullException(nameof(gameServersRepository));
            _gameServerStatusRepository = gameServerStatusRepository ?? throw new ArgumentNullException(nameof(gameServerStatusRepository));
            _rconClientFactory = rconClientFactory ?? throw new ArgumentNullException(nameof(rconClientFactory));
        }

        public override async Task HandleChatMessage(Guid serverId, string name, string guid, string message, ChatType chatType)
        {
            if (!IsMatchingCommand(message))
                return;

            if (!name.Contains(">XI<"))
                return;

            var server = await _gameServersRepository.GetGameServer(serverId);

            var gameServerStatus = await _gameServerStatusRepository.GetStatus(serverId, TimeSpan.Zero);

            var targetName = name;
            if (gameServerStatus != null)
            {
                var splits = message.Replace("!fu", "").Trim().Split(' ');
                if (splits.Any())
                {
                    var potentialTarget = splits.First().ToLower();
                    var potentialMatch = gameServerStatus.Players.Where(p => p.Name.ToLower().Contains(potentialTarget)).ToList();

                    if (potentialMatch.Count == 1) targetName = potentialMatch.First().Name;
                }
            }

            _logger.LogInformation("FuckYou initiated for {name} on {server}", name, server.Title);

            var responseMessage = GenerateResponseMessage(targetName);
            _logger.LogInformation("Executing FuckYou response '{response}' for {name} on {server}", responseMessage, name, server.Title);

            var rconClient = _rconClientFactory.CreateInstance(server.GameType, server.ServerId, server.Hostname, server.QueryPort, server.RconPassword);
            await rconClient.Say(responseMessage);
        }

        private static string GenerateResponseMessage(string name)
        {
            string[] responses =
            {
                "If laughter is the best medicine, your face must be curing the world.",
                "You're so ugly, you scared the crap out of the toilet.",
                "Your family tree must be a cactus because everybody on it is a prick.",
                "No I'm not insulting you, I'm describing you.",
                "If I had a face like yours, I'd sue my parents.",
                "Your birth certificate is an apology letter from the condom factory.",
                "I guess you prove that even god makes mistakes sometimes.",
                "You're so fake, Barbie is jealous.",
                "I’m jealous of people that don’t know you!",
                "If I wanted to kill myself I'd climb your ego and jump to your IQ.",
                "Brains aren't everything. In your case they're nothing.",
                "I don't know what makes you so stupid, but it really works.",
                "I can explain it to you, but I can’t understand it for you.",
                "Roses are red violets are blue, God made me pretty, what happened to you?",
                "Calling you an idiot would be an insult to all the stupid people.",
                "You, sir, are an oxygen thief!",
                "Don't like my sarcasm, well I don't like your stupid.",
                "Why don't you go play in traffic.",
                "Please shut your mouth when you’re talking to me.",
                "I'd slap you, but that would be animal abuse.",
                "Stop trying to be a smart ass, you're just an ass.",
                "The last time I saw something like you, I flushed it.",
                "'m busy now. Can I ignore you some other time?",
                "You have Diarrhea of the mouth; constipation of the ideas.",
                "If ugly were a crime, you'd get a life sentence.",
                "Your mind is on vacation but your mouth is working overtime.",
                "I can lose weight, but you’ll always be ugly.",
                "Why don't you slip into something more comfortable... like a coma.",
                "Shock me, say something intelligent.",
                "If your gonna be two faced, honey at least make one of them pretty.",
                "Keep rolling your eyes, perhaps you'll find a brain back there.",
                "You are not as bad as people say, you are much, much worse.",
                "I don't know what your problem is, but I'll bet it's hard to pronounce.",
                "You get ten times more girls than me? ten times zero is zero...",
                "There is no vaccine against stupidity.",
                "You're the reason the gene pool needs a lifeguard.",
                "Sure, I've seen people like you before - but I had to pay an admission.",
                "How old are you? - Wait I shouldn't ask, you can't count that high.",
                "You're like Monday mornings, nobody likes you.",
                "Of course I talk like an idiot, how else would you understand me?",
                "All day I thought of you... I was at the zoo.",
                "To make you laugh on Saturday, I need to you joke on Wednesday.",
                "You're so fat, you could sell shade.",
                "Don't you need a license to be that ugly?",
                "Your house is so dirty you have to wipe your feet before you go outside.",
                "If you really spoke your mind, you'd be speechless.",
                "Stupidity is not a crime so you are free to go.",
                "You are so old, when you were a kid rainbows were black and white.",
                "If I told you that I have a piece of dirt in my eye, would you move?",
                "You so dumb, you think Cheerios are doughnut seeds.",
                "So, a thought crossed your mind? Must have been a long and lonely journey.",
                "You are so old, your birth-certificate expired.",
                "Every time I'm next to you, I get a fierce desire to be alone.",
                "You're so dumb that you got hit by a parked car.",
                "Keep talking, someday you'll say something intelligent!",
                "You're so fat, you leave footprints in concrete.",
                "How did you get here? Did someone leave your cage open?",
                "Pardon me, but you've obviously mistaken me for someone who gives a damn.",
                "Wipe your mouth, there's still a tiny bit of bullshit around your lips.",
                "Don't you have a terribly empty feeling - in your skull?",
                "As an outsider, what do you think of the human race?",
                "Just because you have one doesn't mean you have to act like one.",
                "We can always tell when you are lying. Your lips move.",
                "Are you always this stupid or is today a special occasion?"
            };

            var rand = new Random();
            var index = rand.Next(responses.Length);

            return $"^6{name} ^7- ^3{responses[index]}";
        }
    }
}