# Raspberry Pi temperature monitor
> for 1-wire

## Asennusohjeet
1. asenna kirjoittamalla `mono install`

```sh
cd RaspTemp
```

##### Git komentoja Raspberryss‰
Kysy status
```sh
git status
```
Lataa muutokset Gitist‰
```sh
git pull
```
Jos git pull ei toimi, voi sinulla olla paikallisia muutoksia,
joita ei voi ylikirjoittaa. Tuhoa paikalliset muutokset komennolla
```sh
git stash
```
Lataa tiedosto "w1_slave" Gittiin_
```sh
git add RaspTemp/w1_slave
```
##### Ohjelman k‰‰nt‰minen ja suoritus
```sh
mcs RaspTemp.cs
mono RaspTemp.exe
```
##### Mik‰li synkronointi Visual Studiolla ei toimi, tee se manuaalisesti
```sh
git add .
git commit -m "Kommentti muutoksista"
Git push -u origin master
```


Lis‰ohjeita https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet
