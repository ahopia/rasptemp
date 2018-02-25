# Raspberry Pi temperature monitor
> for 1-wire

## Asennusohjeet
1. asenna kirjoittamalla `mono install`

```sh
cd RaspTemp
```

##### Git komentoja Raspberryss‰
Kysy status
````
git status
````
Lataa muutokset Gitist‰
````
git pull
````
Lataa tiedosto "w1_slave" Gittiin_
````
git add RaspTemp/w1_slave
````
##### Ohjelman k‰‰nt‰minen ja suoritus
````
mcs RaspTemp.cs
mono RaspTemp.exe
````
##### Mik‰li synkronointi Visual Studiolla ei toimi, tee se manuaalisesti
````
git add .
git commit -m "Kommentti muutoksista"
Git push -U origin master
````


Lis‰ohjeita https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet
