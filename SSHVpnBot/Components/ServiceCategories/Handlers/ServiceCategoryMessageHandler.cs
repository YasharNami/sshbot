using ConnectBashBot.Commons;
using SSHVpnBot.Components.Base;
using SSHVpnBot.Components.Subscribers;
using SSHVpnBot.Repositories.Uw;
using SSHVpnBot.Telegram.Keyboards;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace SSHVpnBot.Components.ServiceCategories.Handlers;

public class ServiceCategoryMessageHandler : MessageHandler
{
    public ServiceCategoryMessageHandler(ITelegramBotClient _bot, Update update, IUnitOfWork _uw,
        Subscriber subscriber, CancellationToken cancellationToken = default)
        :base(_bot,update,_uw,subscriber)
    {
        
    }
    public override async Task MessageHandlerAsync()
    {
        switch (message.Type)
        {
            case MessageType.Text:
                if (step.StartsWith("update*"))
                {
                    var category = await _uw.ServiceCategoryRepository.GetByServiceCategoryCode(step.Split("*")[1]);
                    var property = step.Split("*")[2];
                    switch (property)
                    {
                        case "description":
                            category.Description = message.Text.Fa2En();
                            await _bot.SendTextMessageAsync(user.Id, "توضیحات دسته بندی با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceCategoryRepository.Update(category);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                        case "title":
                            category.Title = message.Text.Fa2En();
                            await _bot.SendTextMessageAsync(user.Id, "نام دسته بندی با موفقیت ویرایش شد ✅",
                                replyMarkup: MarkupKeyboards.Main(subscriber.Role));
                            _uw.ServiceCategoryRepository.Update(category);
                            _uw.SubscriberRepository.ChangeStep(user.Id, $"none");
                            break;
                    }

                    await _bot.DeleteMessageAsync(user.Id, int.Parse(step.Split("*")[3]));

                    await _bot.UpdateCategoryMessage(_uw, user.Id, category);
                }

                break;
        }
    }
}