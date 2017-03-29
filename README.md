# NValidate [![Build Status](https://travis-ci.org/horia141/nvalidate.svg?branch=master)](https://travis-ci.org/horia141/nvalidate)

_Danger_: rough draft ahead.

NValidate is a library for business logic validation. Validation, in this context, is a little bit like testing. But instead of trying to predict how the system will behave, it checks that the system has behaved in a certain way. It's useful for applications with complex and distributed business logic, to try to assert the simple truths underneath.

Consider a scientific bookstore management application. Your entities are books and authors. A book can have multiple authors, and an author can have multiple books. A simple constraint is that a book must have a title of more than 2 characters. A more sophisticated constraint is that each book _must_ have some authors and each author _must_ have some books. A validator for these would look like:

```cs
[ValidatorFixture]
public class BookstoreFixture
{

    [ValidatorTemplate]
    [Projector(typeof(Projectors.ForEachBook))]
    public void BookHasLongTitle(CheckRecorder record, Book book)
    {
        record.That(book.Title, Has.Length.GreaterThan(2));
    }

    [ValidatorTemplate]
    [Projector(typeof(Projectors.ForEachBook))]
    public void BookHasAnAuthor(CheckRecorder record, Book book, IEnumerable<BookAndAuthor> booksAndAuthors) {
        // booksAndAuthors is what you get by just reading in memory a many-to-many table.
        var firstAuthor = booksAndAuthors.FirstOrDefault(ba => ba.BookId = book.Id);
        record.That(firstAuthor, Is.Not.Null);
    }

    [ValidatorTemplate]
    [Projector(typeof(Projectors.ForEachAuthor))]
    public void AuthorHasABook(CheckRecorder record, Author author, IEnumerable<BookAndAuthor> booksAndAuthors) {
        var firstBook = booksAndAuthors.FirstOrDefault(ba => ba.AuthorId = author.Id);
        record.That(firstBook, Is.Not.Null);
    }
}
```
