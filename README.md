# Marvel
Request Marvel API from https://developer.marvel.com/


# Get Single Character
You can search the character's name on search input, but be careful, if you pass the wrong name it returns as not found

![image](https://user-images.githubusercontent.com/54826498/168486785-5950da02-7e12-4876-abe7-f84730fbceb9.png)

Put the character's name in the endpoint after the = 

- Get Single Character Example:
- /Characters/GetCharacter?GetCharacter=wolverine
- https://localhost:7200/Characters/GetCharacter?GetCharacter=wolverine

# Edit Character
To edit you need the character's id, after that just make it available on the endpoint as in the example

- Example:
- /Characters/edit/1009718
- https://localhost:7200/Characters/edit/1009718

# Details From Character
To Detail you need the character's id,

Put the character's id in the endpoint after the =,

After that just make it available on the endpoint as in the example

- Example:
- /Characters/details?id=1009718
- https://localhost:7200/Characters/details?id=1009718
