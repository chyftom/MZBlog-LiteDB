﻿using FluentAssertions;
using LiteDB;
using MZBlog.Core.Commands.Accounts;
using MZBlog.Core.Documents;
using Xunit;

namespace MZBlog.Core.Tests.Accounts
{
    public class ChangePasswordCommandTests : LiteDBBackedTest
    {
        private string authorId = "mzyi";

        [Fact]
        public void change_password_fail_if_old_password_does_not_match()
        {
            var author = new Author()
            {
                Id = authorId,
                Email = "test@mz.yi",
                HashedPassword = Hasher.GetMd5Hash("mzblog")
            };
            using (var _db = new LiteDatabase(_dbConfig.DbPath))
            {
                var authorCol = _db.GetCollection<Author>(DBTableNames.Authors);
                authorCol.Insert(author);
            }
            new ChangePasswordCommandInvoker(_dbConfig)
               .Execute(new ChangePasswordCommand()
               {
                   AuthorId = author.Id,
                   OldPassword = "wrong psw",
                   NewPassword = "pswtest",
                   NewPasswordConfirm = "pswtest"
               })
               .Success.Should().BeFalse();
        }

        [Fact]
        public void change_password()
        {
            var author = new Author()
            {
                Email = "test@mz.yi",
                HashedPassword = Hasher.GetMd5Hash("mzblog")
            };
            using (var _db = new LiteDatabase(_dbConfig.DbPath))
            {
                var authorCol = _db.GetCollection<Author>(DBTableNames.Authors);
                authorCol.Insert(author);

                new ChangePasswordCommandInvoker(_dbConfig)
                    .Execute(new ChangePasswordCommand()
                    {
                        AuthorId = author.Id,
                        OldPassword = "mzblog",
                        NewPassword = "pswtest",
                        NewPasswordConfirm = "pswtest"
                    })
                    .Success.Should().BeTrue();

                authorCol.FindById(author.Id).HashedPassword.Should().BeEquivalentTo(Hasher.GetMd5Hash("pswtest"));
            }
        }

        ~ChangePasswordCommandTests()
        {
            using (var _db = new LiteDatabase(_dbConfig.DbPath))
            {
                var authorCol = _db.GetCollection<Author>(DBTableNames.Authors);
                authorCol.Delete(authorId);
            }
        }
    }
}