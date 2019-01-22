# **Demangler**
AndroidNDKに付属されているc++filtやnmはバグがあり信用できないのでデマングラを実装することになりました。  

**このデマングラは試作です。サポートしないシンボルタイプがあります。使用する際は自己責任でお願いします。**

- [x] name
- [x] template
- [x] nested
- [ ] special-name
- [ ] repeat
- [ ] lambda
- [ ] qualifier
- [ ] return
- [ ] bare-function
- [ ] guard variables
- [ ] vtable
- [ ] VTT
- [ ] virtual thunk
- [ ] non-virtual thunk
- [ ] typeinfo