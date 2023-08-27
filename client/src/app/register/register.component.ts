import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css'],
  providers: [DatePipe]
})
export class RegisterComponent implements OnInit {

    @Output() cancelRegister = new EventEmitter();
    registerForm: FormGroup = new FormGroup({});
    maxDate: Date = new Date();
    validationErrors: string[] | undefined = [];

    constructor(private accountService: AccountService, private formBuilder: FormBuilder, private router: Router, private datePipe: DatePipe) { }

    ngOnInit(): void {
        this.initializeForm();
        this.maxDate.setFullYear(this.maxDate.getFullYear() - 18)
    }

    initializeForm() {
 
        // With formBuilder object reference
        this.registerForm = this.formBuilder.group({
            gender: ['male'],
            username: ['', Validators.required],
            knownAs: ['', Validators.required],
            dateOfBirth: ['', Validators.required],
            city: ['', Validators.required],
            country: ['', Validators.required],
            password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
            confirmPassword: ['', [Validators.required, this.matchValues('password')]]
        });
    
        // Without formBuilder object reference (The two options are valid, but, the first one, is less verbose (more clean))
        // this.registerForm = new FormGroup({
        //     username: new FormControl('Hello', Validators.required),
        //     password: new FormControl('', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]),
        //     confirmPassword: new FormControl('', [Validators.required, this.matchValues('password')]) 
        // });

        this.registerForm.controls['password'].valueChanges.subscribe({
            next: () => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
        })

    }

    matchValues(matchTo: string): ValidatorFn {
        return (control: AbstractControl) => {
            return control.value === control.parent?.get(matchTo)?.value ? null : {noMatching: true}
        };
    }

    register() {
        const dob = this.getDateOnly(this.registerForm.controls['dateOfBirth'].value);  
        const values = {...this.registerForm.value, dateOfBirth: dob};

        this.accountService.register(values).subscribe({
            next: () => {
                this.router.navigateByUrl('/members');
            },
            error: (error) => {
                this.validationErrors = error;
            }
        });
    }

    cancel() {
        this.cancelRegister.emit(false);
    }

    private getDateOnly(dob: string | undefined) {
        
        if(!dob) return;
        let theDob = new Date(dob);
        let returnValue = new Date(theDob.setMinutes(theDob.getMinutes()-theDob.getTimezoneOffset())).toString().slice(0, 10);

        return this.datePipe.transform(returnValue, 'yyyy-MM-dd');
    }
}

