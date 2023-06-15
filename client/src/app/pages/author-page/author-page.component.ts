import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { catchError, throwError } from 'rxjs';
import { AuthorEditDialogData } from 'src/app/components/author-edit/author-edit-dialog-data';
import { AuthorEditComponent } from 'src/app/components/author-edit/author-edit.component';
import { AuthorListDataSource } from 'src/app/components/author-list/author-list-datasource';
import { Author } from 'src/app/models/author';
import { AuthorService } from 'src/app/services/author/author.service';

@Component({
  selector: 'app-author-page',
  templateUrl: './author-page.component.html',
  styleUrls: ['./author-page.component.scss'],
})
export class AuthorPageComponent implements OnInit {
  public dataSource = new AuthorListDataSource();
  private _authors = new Array<Author>();

  constructor(
    private _service: AuthorService,
    private _snackBar: MatSnackBar,
    public dialog: MatDialog
  ) {}

  public ngOnInit(): void {
    this._service
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
        this.updateDataSource();
      });
  }

  public onAdd(): void {
    const dialogRef = this.dialog.open<
      AuthorEditComponent,
      AuthorEditDialogData,
      AuthorEditDialogData
    >(AuthorEditComponent, {
      data: {},
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && result.author) {
        this._service
          .addAuthor(result.author)
          .pipe(
            catchError((err) => {
              this._snackBar.open('Ошибка!', 'OK');
              console.error(err);
              return throwError(() => err);
            })
          )
          .subscribe((a) => {
            this._authors.push(a);
            this.updateDataSource();
            this._snackBar.open('Запись добавлена', 'ОК');
          });
      }
    });
  }

  public onEdit(author: Author): void {
    const dialogRef = this.dialog.open<
      AuthorEditComponent,
      AuthorEditDialogData,
      AuthorEditDialogData
    >(AuthorEditComponent, {
      data: { author },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result && result.author) {
        this._service.updateAuthor(result.author).subscribe((a) => {
          const foundIndex = this._authors.findIndex(
            (author) => author.id === a.id
          );
          if (foundIndex > 0) {
            this._authors[foundIndex] = a;
            this.updateDataSource();
            this._snackBar.open('Запись обновлена', 'ОК');
          }
        });
      }
    });
  }

  public onDelete(id: string): void {
    this._service
      .removeAuthor(id)
      .pipe(
        catchError((err) => {
          this._snackBar.open('Ошибка!', 'OK');
          console.error(err);
          return throwError(() => err);
        })
      )
      .subscribe(() => {
        this._authors = this._authors.filter((b) => b.id !== id);
        this.updateDataSource();
        this._snackBar.open('Запись успешно удалена', 'ОК');
      });
  }
  public updateDataSource(): void {
    this.dataSource.setData(this._authors);
  }
}
