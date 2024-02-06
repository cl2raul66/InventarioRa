using CommunityToolkit.Mvvm.Messaging.Messages;
using InventarioRa.Models;

namespace InventarioRa.Tools.Messages;

public class SendArticleentryChangedMessage(ArticleEntry value) : ValueChangedMessage<ArticleEntry>(value) { }

public class SendDispatchChangedMessage(Dispatch value) : ValueChangedMessage<Dispatch>(value) { }

public class SearchInventoryForSearchChangedMessage(string value) : ValueChangedMessage<string>(value) { }

public class SendDispatchForSearchChangedMessage(DispatchForSearch value) : ValueChangedMessage<DispatchForSearch>(value) { }
