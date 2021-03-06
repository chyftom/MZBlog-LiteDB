﻿using LiteDB;
using MZBlog.Core.Documents;
using System;
using System.ComponentModel.DataAnnotations;

namespace MZBlog.Core.Commands.Posts
{
    public class NewCommentCommand
    {
        public NewCommentCommand()
        {
            Id = ObjectId.NewObjectId();
        }

        public string Id { get; set; }

        public SpamShield SpamShield { get; set; }

        public string PostId { get; set; }

        [Required]
        public string NickName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string SiteUrl { get; set; }

        [Required]
        [MinLength(3)]
        public string Content { get; set; }

        public string IPAddress { get; set; }
    }

    public class NewCommentCommandInvoker : ICommandInvoker<NewCommentCommand, CommandResult>
    {
        private readonly Config _dbConfig;
        private readonly ISpamShieldService _spamShield;

        public NewCommentCommandInvoker(Config dbConfig, ISpamShieldService spamShield)
        {
            _dbConfig = dbConfig;
            _spamShield = spamShield;
        }

        public CommandResult Execute(NewCommentCommand command)
        {
            if (_spamShield.IsSpam(command.SpamShield))
            {
                return new CommandResult("You are a spam!");
            }

            var comment = new BlogComment
            {
                Id = command.Id,
                Email = command.Email,
                NickName = command.NickName,
                Content = command.Content,
                IPAddress = command.IPAddress,
                PostId = command.PostId,
                SiteUrl = command.SiteUrl,
                CreatedTime = DateTime.UtcNow
            };
            using (var _db = new LiteDatabase(_dbConfig.DbPath))
            {
                var blogCommentCol = _db.GetCollection<BlogComment>(DBTableNames.BlogComments);
                var result = blogCommentCol.Insert(comment);

                return CommandResult.SuccessResult;
            }
        }
    }
}