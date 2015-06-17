﻿using LiteDB;
using MZBlog.Core.Documents;

namespace MZBlog.Core.Commands.Accounts
{
    public class ChangePasswordCommand
    {
        public string AuthorId { get; set; }

        public string NewPassword { get; set; }

        public string NewPasswordConfirm { get; set; }

        public string OldPassword { get; set; }
    }

    public class ChangePasswordCommandInvoker : ICommandInvoker<ChangePasswordCommand, CommandResult>
    {
        private readonly LiteDatabase _db;

        public ChangePasswordCommandInvoker(LiteDatabase db)
        {
            _db = db;
        }

        public CommandResult Execute(ChangePasswordCommand command)
        {
            var author = _db.GetCollection<Author>(DBTableNames.Authors).FindById(command.AuthorId);
            if (Hasher.GetMd5Hash(command.OldPassword) != author.HashedPassword)
            {
                return new CommandResult("旧密码不正确!");
            }

            author.HashedPassword = Hasher.GetMd5Hash(command.NewPassword);
            _db.GetCollection<Author>(DBTableNames.Authors).Update(author);
            return CommandResult.SuccessResult;
        }
    }
}