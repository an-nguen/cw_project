import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { DomSanitizer } from '@angular/platform-browser';
import * as FileSaver from 'file-saver';
import { catchError, throwError } from 'rxjs';
import { BookEditDialogData } from 'src/app/components/book-edit/book-edit-dialog-data';
import { BookEditComponent } from 'src/app/components/book-edit/book-edit.component';
import { BookListDataSource } from 'src/app/components/book-list/book-list-datasource';
import { UploadBookFileData } from 'src/app/components/upload-book-file/upload-book-file-data';
import { UploadBookFileResponse } from 'src/app/components/upload-book-file/upload-book-file-response';
import { UploadBookFileComponent } from 'src/app/components/upload-book-file/upload-book-file.component';
import { Author } from 'src/app/models/author';
import { Book } from 'src/app/models/book';
import { AuthorService } from 'src/app/services/author/author.service';
import { BookService } from 'src/app/services/book/book.service';

@Component({
  selector: 'app-book-page',
  templateUrl: './book-page.component.html',
  styleUrls: ['./book-page.component.scss'],
})
export class BookPageComponent implements OnInit {
  public dataSource = new BookListDataSource();
  private _books = new Array<Book>();
  private _authors = new Array<Author>();

  constructor(
    private _bookService: BookService,
    private _authorService: AuthorService,
    private _snackBar: MatSnackBar,
    private _dialog: MatDialog
  ) {}

  public ngOnInit(): void {
    this._bookService
      .getBooks()
      .pipe(
        catchError((err) => {
          this._snackBar.open('Ошибка!', 'OK');
          console.error(err);
          return throwError(() => err);
        })
      )
      .subscribe((books) => {
        this._books = books;
        this.updateDataSource();
      });
    this._authorService
      .getAuthors()
      .pipe(
        catchError((err) => {
          this._snackBar.open('Ошибка!', 'OK');
          console.error(err);
          return throwError(() => err);
        })
      )
      .subscribe((authors) => {
        this._authors = authors;
      });
  }

  public updateDataSource(): void {
    this.dataSource.setData(this._books);
  }

  public onAdd(): void {
    const dialogRef = this._dialog.open<
      BookEditComponent,
      BookEditDialogData,
      BookEditDialogData
    >(BookEditComponent, {
      data: {
        authors: this._authors,
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && result.book) {
        if (result.book.authors) {
          result.book.authorIds = result.book.authors.map((a) => a.id || '');
        }
        this._bookService
          .addBook(result.book)
          .pipe(
            catchError((err) => {
              this._snackBar.open('Ошибка!', 'OK');
              console.error(err);
              return throwError(() => err);
            })
          )
          .subscribe((book) => {
            this._books.push(book);
            this.updateDataSource();
            this._snackBar.open('Запись добавлена', 'ОК');
          });
      }
    });
  }

  public onEdit(book: Book): void {
    const dialogRef = this._dialog.open<
      BookEditComponent,
      BookEditDialogData,
      BookEditDialogData
    >(BookEditComponent, {
      data: { book, authors: this._authors },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && result.book) {
        if (result.book.authors) {
          result.book.authorIds = result.book.authors.map((a) => a.id || '');
        }
        this._bookService
          .updateBook(result.book)
          .pipe(
            catchError((err) => {
              this._snackBar.open('Ошибка!', 'OK');
              console.error(err);
              return throwError(() => err);
            })
          )
          .subscribe((b) => {
            const foundIndex = this._books.findIndex(
              (book) => book.id === b.id
            );
            if (foundIndex > 0) {
              this._books[foundIndex] = b;
              this.updateDataSource();
              this._snackBar.open('Запись обновлена', 'ОК');
            }
          });
      }
    });
  }

  public onDelete(id: string): void {
    this._bookService
      .removeBook(id)
      .pipe(
        catchError((err) => {
          this._snackBar.open('Ошибка!', 'OK');
          console.error(err);
          return throwError(() => err);
        })
      )
      .subscribe(() => {
        this._books = this._books.filter((b) => b.id !== id);
        this.updateDataSource();
        this._snackBar.open('Запись успешно удалена', 'ОК');
      });
  }

  public onUpload(id: string): void {
    const dialogRef = this._dialog.open<
      UploadBookFileComponent,
      UploadBookFileData,
      UploadBookFileResponse
    >(UploadBookFileComponent, {
      data: { id },
    });

    dialogRef.afterClosed().subscribe((response) => {
      if (response) {
        this._bookService
          .uploadBook(id, response.file)
          .pipe(
            catchError((err) => {
              this._snackBar.open('Ошибка!', 'OK');
              console.error(err);
              return throwError(() => err);
            })
          )
          .subscribe(() => {
            const found = this._books.find((b) => b.id === id);
            if (found) {
              found.hasFile = true;
            }
            this.updateDataSource();
            this._snackBar.open('Загрузка файла прошла успешно', 'ОК');
          });
      }
    });
  }
  public onDownload(id: string): void {
    this._bookService.downloadBook(id).subscribe((response) => {
      let filename = '';
      const disposition = response.headers.get('Content-Disposition');
      if (disposition && disposition.indexOf('attachment') !== -1) {
        const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
        const matches = filenameRegex.exec(disposition);
        if (matches != null && matches[1]) {
          filename = matches[1].replace(/['"]/g, '');
        }
      }

      if (response.body) FileSaver.saveAs(response.body, filename);
    });
  }
}
