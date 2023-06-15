import { Component, OnInit } from '@angular/core';
import { Book } from 'src/app/models/book';
import { BookService } from 'src/app/services/book/book.service';

@Component({
  selector: 'app-book-page',
  templateUrl: './book-page.component.html',
  styleUrls: ['./book-page.component.scss'],
})
export class BookPageComponent implements OnInit {
  public books: Array<Book> = [];

  constructor(private _bookService: BookService) {}

  public ngOnInit(): void {
    this._bookService.getBooks().subscribe((books) => {
      this.books = books;
    });
  }

  public onEdit(book: Book): void {}

  public onDelete(id: string): void {
    this._bookService.removeBook(id).subscribe(() => {
      this.books = this.books.filter((b) => b.id !== id);
    });
  }
}
