import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { AuthorEditDialogData } from './author-edit-dialog-data';
import { FormControl, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-author-edit',
  templateUrl: './author-edit.component.html',
  styleUrls: ['./author-edit.component.scss'],
})
export class AuthorEditComponent {
  public authorForm = new FormGroup({
    lastName: new FormControl<string>('', [
      Validators.required,
      Validators.minLength(2),
    ]),
    firstName: new FormControl<string>('', [
      Validators.required,
      Validators.minLength(2),
    ]),
    midName: new FormControl<string | undefined>(''),
  });

  constructor(
    private _dialogRef: MatDialogRef<AuthorEditComponent, AuthorEditDialogData>,
    @Inject(MAT_DIALOG_DATA) public dialogData: AuthorEditDialogData
  ) {
    if (dialogData.author) {
      this.authorForm.setValue({
        lastName: dialogData.author.lastName,
        firstName: dialogData.author.firstName,
        midName: dialogData.author.midName,
      });
    }
  }

  public onEdit() {
    if (this.authorForm.valid)
      this._dialogRef.close({
        author: {
          id: this.dialogData.author?.id,
          lastName: this.authorForm.get('lastName')!.value!,
          firstName: this.authorForm.get('firstName')!.value!,
          midName: this.authorForm.get('midName')?.value ?? undefined,
        },
      });
  }

  public onClose() {
    this._dialogRef.close(undefined);
  }
}
