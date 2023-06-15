import {
  Component,
  ElementRef,
  Inject,
  OnInit,
  ViewChild,
} from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { BookEditDialogData } from './book-edit-dialog-data';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { Author } from 'src/app/models/author';
import { Observable, map, startWith } from 'rxjs';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatChipInputEvent } from '@angular/material/chips';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';

@Component({
  selector: 'app-book-edit',
  templateUrl: './book-edit.component.html',
  styleUrls: ['./book-edit.component.scss'],
})
export class BookEditComponent implements OnInit {
  public titleFormCtrl: FormControl<string | null> = new FormControl('', [
    Validators.required,
    Validators.minLength(1),
  ]);

  public isbnFormCtrl: FormControl<string | null> = new FormControl('');
  public authorsFormCtrl: FormControl<string | null> = new FormControl('');

  public separatorKeysCodes: number[] = [ENTER, COMMA];

  public bookForm = new FormGroup({
    title: this.titleFormCtrl,
    isbn: this.isbnFormCtrl,
    authors: this.authorsFormCtrl,
  });
  public filteredOptions?: Observable<Author[]>;
  public selectedOptions: Author[] = [];

  @ViewChild('authorInput') authorInput!: ElementRef<HTMLInputElement>;

  constructor(
    private _dialogRef: MatDialogRef<BookEditComponent, BookEditDialogData>,
    @Inject(MAT_DIALOG_DATA) public dialogData: BookEditDialogData
  ) {}

  public ngOnInit(): void {
    if (this.dialogData.book) {
      this.bookForm.setValue({
        title: this.dialogData.book.title,
        isbn: this.dialogData.book.isbn,
        authors: null,
      });
      if (this.dialogData.book.authors) {
        this.selectedOptions.push(...this.dialogData.book.authors);
      }
    }
    this.filteredOptions = this.bookForm.get('authors')?.valueChanges.pipe(
      startWith(null),
      map((value) => {
        if (typeof value === 'string') {
          return this._filter(value as string);
        } else {
          return this._getAllAuthors().slice();
        }
      })
    );
  }

  public displayFn(author: Author): string {
    return author.firstName + ' ' + author.lastName;
  }

  public selectedAuthor(event: MatAutocompleteSelectedEvent) {
    this.selectedOptions.push(event.option.value);
    this.authorInput.nativeElement.value = '';
    this.authorsFormCtrl.setValue(null);
  }

  public addAuthor(event: MatChipInputEvent) {
    const value = event.value ?? undefined;
    if (value) {
      this.selectedOptions.push();
    }

    event.chipInput!.clear();

    this.authorsFormCtrl.setValue(null);
  }

  public removeAuthor(author: Author) {
    const index = this.selectedOptions.indexOf(author);

    if (index >= 0) {
      this.selectedOptions.splice(index, 1);
    }
  }

  public onEdit() {
    if (this.bookForm.valid) {
      this._dialogRef.close({
        book: {
          id: this.dialogData.book?.id,
          title: this.titleFormCtrl.value!,
          isbn: this.isbnFormCtrl.value ?? '',
          authors: this.selectedOptions,
        },
        authors: [],
      });
    }
  }

  public onClose() {
    this._dialogRef.close(undefined);
  }

  private _filter(name: string): Author[] {
    const filterValue = name.toLowerCase();

    return this._getAllAuthors().filter(
      (option) =>
        option.firstName.toLowerCase().includes(filterValue) ||
        option.lastName.toLowerCase().includes(filterValue) ||
        option.midName?.toLowerCase().includes(filterValue)
    );
  }
  private _getAllAuthors(): Author[] {
    const selectedAuthorIds = this.selectedOptions.map(
      (selected) => selected.id
    );
    return this.dialogData.authors.filter(
      (a) => !selectedAuthorIds.includes(a.id)
    );
  }
}
